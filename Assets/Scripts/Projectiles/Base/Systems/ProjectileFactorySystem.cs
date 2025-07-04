using SpaceGame.Combat.Components;
using SpaceGame.Movement.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SpaceGame.Combat.Systems
{
    [UpdateInGroup(typeof(CombatLateUpdateGroup))]
    partial struct ProjectileFactorySystem : ISystem
    {
        NativeParallelHashMap<FixedString32Bytes, NativeList<Entity>> pools;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            pools = new NativeParallelHashMap<FixedString32Bytes, NativeList<Entity>>(16, Allocator.Persistent);

            state.RequireForUpdate<ProjectileSpawnRequest>();
            state.RequireForUpdate<ProjectilePrefabLookupSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<ProjectilePrefabLookupSingleton>(out var blobSingleton))
                return;

            if (!SystemAPI.TryGetSingletonBuffer<ProjectileSpawnRequest>(out var bulletSpawnCollector))
                return;

            ref var lookup = ref blobSingleton.Lookup.Value;
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (bulletId, entity) in SystemAPI.Query<RefRO<ProjectileId>>()
                .WithOptions(EntityQueryOptions.IncludeDisabledEntities)
                .WithAll<Disabled, NeedsPoolingTag, ProjectileTag>()
                .WithEntityAccess())
            {
                var pool = GetOrCreatePool(bulletId.ValueRO.Value);
                pool.Add(entity);
                ecb.RemoveComponent<NeedsPoolingTag>(entity);
            }

            foreach (var request in bulletSpawnCollector)
            {
                var prefabData = lookup.GetPrefab(request.BulletId);

                if (prefabData.Entity == Entity.Null || !state.EntityManager.Exists(prefabData.Entity))
                {
                    UnityEngine.Debug.LogWarning($"Projectile prefab doesn't exist: {request.BulletId}");
                    continue;
                }

                var projectileEntity = GetFromPool(state.EntityManager, ecb, prefabData.Id, prefabData.Entity);

                var projectileScale = state.EntityManager.HasComponent<ProjectileScale>(prefabData.Entity) ? state.EntityManager.GetComponentData<ProjectileScale>(prefabData.Entity).Value : 1.0f;
                var speed = state.EntityManager.GetComponentData<ThrustSettings>(prefabData.Entity);

                float angle = math.atan2(request.Heading.y, request.Heading.x);
                quaternion flatRotation = quaternion.RotateZ(angle);

                state.EntityManager.SetComponentData(projectileEntity, new LocalTransform
                {
                    Position = request.Position,
                    Rotation = flatRotation,
                    Scale = projectileScale * request.ParentScale
                });

                var lifeTime = request.Range / speed.MaxSpeed;

                state.EntityManager.SetComponentData(projectileEntity, new Target() { Value = request.Target });
                state.EntityManager.SetComponentData(projectileEntity, new Lifetime { Value = lifeTime });
                state.EntityManager.SetComponentData(projectileEntity, new Heading() { Value = request.Heading });
                state.EntityManager.SetComponentData(projectileEntity, new PreviousPosition() { Value = request.Position });
                state.EntityManager.SetComponentData(projectileEntity, new Damage() { Value = request.Damage, Type = request.DamageType });
                state.EntityManager.SetComponentData(projectileEntity, new TeamTag() { Team = request.Team });
                state.EntityManager.SetComponentData(projectileEntity, new ProjectileId() { Value = prefabData.Id });
            }

            bulletSpawnCollector.Clear();

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        NativeList<Entity> GetOrCreatePool(FixedString32Bytes id)
        {
            if (!pools.TryGetValue(id, out var list))
            {
                list = new NativeList<Entity>(Allocator.Persistent);
                pools[id] = list;
            }
            return list;
        }

        Entity GetFromPool(EntityManager em, EntityCommandBuffer ecb, FixedString32Bytes bulletId, Entity prefab)
        {
            var pool = GetOrCreatePool(bulletId);

            if (pool.Length == 0)
            {
                AddEntitiesToPool(pool, prefab, em, ecb);
                return TakeEntityFromPool(ecb, em, pool);
            }

            if (pool[pool.Length - 1].Index < 0)
                return TakeEntityFromPool(ecb, em, pool);

            if (em.HasComponent<Disabled>(pool[pool.Length - 1]))
            {
                return TakeEntityFromPool(ecb, em, pool);
            }
            else
            {
                AddEntitiesToPool(pool, prefab, em, ecb);
                return TakeEntityFromPool(ecb, em, pool);
            }
        }

        Entity TakeEntityFromPool(EntityCommandBuffer ecb, EntityManager em, NativeList<Entity> pool)
        {
            var entity = pool[pool.Length - 1];
            pool.RemoveAt(pool.Length - 1);
            EnableEntityAndChildren(ecb, entity, em);
            return entity;
        }


        private void AddEntitiesToPool(NativeList<Entity> pool, Entity prefab, EntityManager em, EntityCommandBuffer ecb)
        {
            for (int i = 0; i < 100; i++)
            {
                var entity = em.Instantiate(prefab);
                DisableEntityAndChildren(ecb, entity, em);
                pool.Add(entity);
            }
        }
        void DisableEntityAndChildren(EntityCommandBuffer ecb, Entity root, EntityManager em)
        {
            if (em.HasBuffer<LinkedEntityGroup>(root))
            {
                var group = em.GetBuffer<LinkedEntityGroup>(root);
                for (int i = 0; i < group.Length; i++)
                {
                    ecb.AddComponent<Disabled>(group[i].Value);
                }
            }
            else
            {
                ecb.AddComponent<Disabled>(root);
            }
        }

        void EnableEntityAndChildren(EntityCommandBuffer ecb, Entity root, EntityManager em)
        {
            if (em.HasBuffer<LinkedEntityGroup>(root))
            {
                var group = em.GetBuffer<LinkedEntityGroup>(root);
                for (int i = 0; i < group.Length; i++)
                {
                    ecb.RemoveComponent<Disabled>(group[i].Value);
                }
            }
            else
            {
                ecb.RemoveComponent<Disabled>(root);
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            foreach (var pool in pools)
            {
                pool.Value.Dispose();
            }

            pools.Dispose();
        }
    }
}
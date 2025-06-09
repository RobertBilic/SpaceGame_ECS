using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Burst;
using UnityEngine;
using SpaceGame.Combat.Components;

namespace SpaceGame.Combat.Systems
{
    [UpdateInGroup(typeof(CombatLateUpdateGroup))]
    public partial class ImpactEffectSpawnSystem : SystemBase
    {
        private bool initialized;
        private Unity.Mathematics.Random random;
        NativeParallelHashMap<FixedString32Bytes, NativeList<Entity>> pools;

        protected override void OnCreate()
        {
            pools = new NativeParallelHashMap<FixedString32Bytes, NativeList<Entity>>(16, Allocator.Persistent);
            random = Unity.Mathematics.Random.CreateFromIndex(5124);
        }

        protected override void OnUpdate()
        {
            if (!SystemAPI.TryGetSingletonBuffer<ImpactSpawnRequest>(out var spawnRequests))
                return;

            if (!SystemAPI.TryGetSingleton<ProjectilePrefabLookupSingleton>(out var blobSingleton))
                return;

            if (SystemAPI.TryGetSingleton<HitEffectEnabled>(out var hitEffectsEnabled))
            {
                if (!hitEffectsEnabled.Enabled)
                {
                    spawnRequests.Clear();
                    return;
                }
            }

            ref var lookup = ref blobSingleton.Lookup.Value;

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            if (!initialized)
            {
                initialized = true;

                for (int i = 0; i < lookup.Entries.Length; i++)
                {
                    var entry = lookup.Entries[i];

                    if (!SystemAPI.HasComponent<OnHitEffectPrefab>(entry.Entity))
                        continue;

                    var pool = GetOrCreatePool(entry.Id);
                    var onHitPrefab = SystemAPI.GetComponent<OnHitEffectPrefab>(entry.Entity);

                    for (int j = 0; j < 5; j++)
                    {
                        AddEntitiesToPool(pool, onHitPrefab.Value, EntityManager, ecb);
                    }
                }
            }

            foreach (var (bulletId, entity) in SystemAPI.Query<RefRO<ProjectileId>>()
                .WithOptions(EntityQueryOptions.IncludeDisabledEntities)
                .WithAll<Disabled, NeedsPoolingTag, ImpactParticle>()
                .WithEntityAccess())
            {
                var pool = GetOrCreatePool(bulletId.ValueRO.Value);
                pool.Add(entity);
                ecb.RemoveComponent<NeedsPoolingTag>(entity);
            }


            for (int i = 0; i < spawnRequests.Length; i++)
            {
                var request = spawnRequests[i];
                var bulletPrefab = lookup.GetPrefab(request.PrefabId);

                if (!SystemAPI.HasComponent<OnHitEffectPrefab>(bulletPrefab.Entity))
                    continue;

                var onHitPrefab = SystemAPI.GetComponent<OnHitEffectPrefab>(bulletPrefab.Entity);

                for (int j = 0; j < request.Count; j++)
                {
                    var particle = GetFromPool(EntityManager, ecb, bulletPrefab.Id, onHitPrefab.Value);

                    ecb.SetComponent(particle, new LocalTransform
                    {
                        Position = request.Position,
                        Rotation = random.NextQuaternionRotation(),
                        Scale = request.Scale
                    }); ;

                    ecb.SetComponent(particle, new ImpactParticle
                    {
                        Lifetime = onHitPrefab.Lifetime,
                        Age = 0f,
                    });

                    ecb.SetComponent(particle, new ProjectileId() { Value = request.PrefabId });
                }
            }

            spawnRequests.Clear();
            ecb.Playback(EntityManager);
            ecb.Dispose();
        }

        protected override void OnDestroy()
        {
            foreach (var pool in pools)
            {
                pool.Value.Dispose();
            }

            pools.Dispose();
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
    }
}
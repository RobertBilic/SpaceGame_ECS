using SpaceGame.Combat.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SpaceGame.Combat.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(CombatSystemGroup))]
    [UpdateAfter(typeof(TurretTargetingSystem))]
    partial struct TurretFiringSystem : ISystem
    {
        BulletPrefabData prefabData;
        bool isInitialized;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TurretTag>();
            state.RequireForUpdate<Target>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!isInitialized)
            {
                foreach (var bulletPrefabData in SystemAPI.Query<RefRO<BulletPrefabData>>().WithOptions(EntityQueryOptions.IncludePrefab))
                {
                    this.prefabData = bulletPrefabData.ValueRO;
                    isInitialized = true;
                    break;
                }
            }


            if (!isInitialized)
                return;

            EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            foreach (var (turretFiringAspect, entity) in SystemAPI.Query<TurretFiringAspect>()
                    .WithAll<TurretTag>()
                    .WithEntityAccess())
            {
                var lastFireTime = turretFiringAspect.LastFireTime;
                var isAlive = turretFiringAspect.IsAlive;
                var firingRate = turretFiringAspect.FiringRate;
                var target = turretFiringAspect.Target;
                var worldTransform = turretFiringAspect.WorldTransform;
                var heading = turretFiringAspect.Heading;
                var range = turretFiringAspect.Range;
                var spawnOffset = turretFiringAspect.BulletSpawnOffsets;
                var rotationBase = turretFiringAspect.RotationBaseReference;
                var damage = turretFiringAspect.Damage;

                double elapsedSinceLastFire = SystemAPI.Time.ElapsedTime - lastFireTime.ValueRW.Value;

                if (!isAlive.ValueRO.Value)
                    continue;

                if (elapsedSinceLastFire < (1f / firingRate.ValueRO.Value))
                    continue;

                if (target.ValueRO.Value == Entity.Null || !SystemAPI.Exists(target.ValueRO.Value))
                    continue;

                if (state.EntityManager.HasComponent<IsAlive>(target.ValueRO.Value))
                {
                    var targetIsAlive = state.EntityManager.GetComponentData<IsAlive>(target.ValueRO.Value);

                    if (!targetIsAlive.Value)
                    {
                        continue;
                    }
                }


                float3 turretPosition = worldTransform.ValueRO.Position;
                float3 turretForward = heading.ValueRO.Value;
                float3 targetPosition = SystemAPI.GetComponent<LocalToWorld>(target.ValueRO.Value).Position;
                float3 toTarget = math.normalize(targetPosition - turretPosition);

                float alignment = math.dot(turretForward, toTarget);

                if (alignment < 0.95f)
                    continue;


                var lifeTime = range.ValueRO.Value / prefabData.BulletSpeed;
                lastFireTime.ValueRW.Value = SystemAPI.Time.ElapsedTime;

                var rotationBaseLocalToWorld = SystemAPI.GetComponent<LocalToWorld>(rotationBase.ValueRO.RotationBase);

                foreach (var offset in spawnOffset)
                {
                    float3 spawnPosition = math.transform(rotationBaseLocalToWorld.Value, offset.Value);

                    var bulletEntity = ecb.Instantiate(prefabData.Entity);

                    ecb.SetComponent(bulletEntity, new LocalTransform
                    {
                        Position = spawnPosition,
                        Rotation = quaternion.identity,
                        Scale = prefabData.Scale
                    });

                    ecb.AddComponent(bulletEntity, new BulletTag());
                    ecb.AddComponent(bulletEntity, new Lifetime { Value = lifeTime });
                    ecb.AddComponent(bulletEntity, new MoveSpeed() { Value = prefabData.BulletSpeed });
                    ecb.AddComponent(bulletEntity, new Heading() { Value = heading.ValueRO.Value });
                    ecb.AddComponent(bulletEntity, new Radius() { Value = prefabData.Scale });
                    ecb.AddComponent(bulletEntity, new PreviousPosition() { Value = spawnPosition });
                    ecb.AddComponent(bulletEntity, new Damage() { Value = damage.ValueRO.Value });
                }

                if (SystemAPI.HasComponent<BarrelRecoilReference>(entity))
                {
                    var recoilData = SystemAPI.GetComponentRO<BarrelRecoilReference>(entity).ValueRO;
                    var recoilEntity = recoilData.Entity;

                    if (SystemAPI.HasComponent<BarrelRecoil>(recoilEntity))
                    {
                        var recoil = SystemAPI.GetComponent<BarrelRecoil>(recoilEntity);
                        recoil.CurrentTime = 0f;
                        ecb.SetComponent(recoilEntity, recoil);
                    }
                    else
                    {
                        ecb.AddComponent(recoilEntity, new BarrelRecoil
                        {
                            CurrentTime = 0f,
                            Duration = recoilData.Duration,
                            MaxDistance = recoilData.MaxDistance,
                            DefaultPosition = recoilData.DefaultPosition
                        });
                    }
                }
            }

            if (ecb.ShouldPlayback)
            {
                ecb.Playback(state.EntityManager);
                ecb.Dispose();
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}
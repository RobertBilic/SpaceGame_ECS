using SpaceGame.Animations.Components;
using SpaceGame.Combat.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SpaceGame.Combat.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(CombatFiringGroup))]
    partial struct TurretFiringSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<TurretTag>();
            state.RequireForUpdate<Target>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            foreach (var (turretFiringAspect, entity) in SystemAPI.Query<TurretFiringAspect>()
                    .WithAll<TurretTag>()
                    .WithEntityAccess())
            {
                var lastFireTime = turretFiringAspect.LastFireTime;
                var firingRate = turretFiringAspect.FiringRate;
                var target = turretFiringAspect.Target;
                var worldTransform = turretFiringAspect.WorldTransform;
                var heading = turretFiringAspect.Heading;
                var range = turretFiringAspect.Range;
                var spawnOffset = turretFiringAspect.BulletSpawnOffsets;
                var rotationBase = turretFiringAspect.RotationBaseReference;
                var damage = turretFiringAspect.Damage;
                var teamTag = turretFiringAspect.TeamTag.ValueRO;
                var bulletId = turretFiringAspect.BulletId.ValueRO;

                double elapsedSinceLastFire = SystemAPI.Time.ElapsedTime - lastFireTime.ValueRW.Value;


                if (elapsedSinceLastFire < (1f / firingRate.ValueRO.Value))
                    continue;

                if (target.ValueRO.Value == Entity.Null || !SystemAPI.Exists(target.ValueRO.Value))
                    continue;

                if (!SystemAPI.HasComponent<LocalToWorld>(target.ValueRO.Value))
                    continue;

                if (state.EntityManager.HasComponent<IsAlive>(target.ValueRO.Value))
                {
                    var targetIsAlive = state.EntityManager.HasComponent<IsAlive>(target.ValueRO.Value);

                    if (!targetIsAlive)
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

                lastFireTime.ValueRW.Value = SystemAPI.Time.ElapsedTime;

                var rotationBaseLocalToWorld = SystemAPI.GetComponent<LocalToWorld>(rotationBase.ValueRO.RotationBase);

                foreach (var offset in spawnOffset)
                {
                    float3 spawnPosition = math.transform(rotationBaseLocalToWorld.Value, offset.Value);

                    if (SystemAPI.TryGetSingletonBuffer<BulletSpawnRequest>(out var buffer))
                    {
                        buffer.Add(new BulletSpawnRequest()
                        {
                            BulletId = bulletId.Value,
                            Damage = damage.ValueRO.Value,
                            Heading = heading.ValueRO.Value,
                            Position = spawnPosition,
                            Range = range.ValueRO.Value,
                            Team = teamTag.Team
                        });
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning($"BulletSpawnRequest doesnt exist");
                    }
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
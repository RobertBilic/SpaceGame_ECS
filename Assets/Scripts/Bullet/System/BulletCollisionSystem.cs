using SpaceGame.Combat.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SpaceGame.Combat.Systems
{
    public struct BulletCollisionDetector : ISpatialQueryCollector
    {
        public bool isEnemyHit;
        public Entity HitEntity;

        private int team;
        private EntityManager em;

        private float3 startR;
        private float3 endR;

        private float3 startL;
        private float3 endL;

        private float3 startU;
        private float3 endU;

        private float3 startD;
        private float3 endD;


        public BulletCollisionDetector(EntityManager manager, int team, float3 startR, float3 endR, float3 startL, float3 endL, float3 startU, float3 endU, float3 startD, float3 endD) : this()
        {
            this.team = team;
            this.em = manager;
            this.startR = startR;
            this.endR = endR;
            this.startL = startL;
            this.endL = endL;
            this.startU = startU;
            this.endU = endU;
            this.startD = startD;
            this.endD = endD;
        }

        public void OnVisitCell(in SpatialDatabaseCell cell, in UnsafeList<SpatialDatabaseElement> elements, out bool shouldEarlyExit)
        {
            shouldEarlyExit = false;

            for (int i = cell.StartIndex; i < cell.StartIndex + cell.ElementsCount; i++)
            {
                var element = elements[i];

                if (em.HasComponent<TeamTag>(element.Entity))
                {
                    var targetTeam = em.GetComponentData<TeamTag>(element.Entity);
                    if (targetTeam.Team == team)
                        continue;
                }

                var shipTransform = em.GetComponentData<LocalTransform>(element.Entity);
                var hitboxes = em.GetBuffer<HitBoxElement>(element.Entity);
                var worldTransform = em.GetComponentData<LocalToWorld>(element.Entity);

                float3 shipWorldPos = shipTransform.Position;
                quaternion shipWorldRot = shipTransform.Rotation;

                float3 shipForward = math.mul(shipWorldRot, new float3(1f, 0f, 0f));
                shipForward.z = 0f;
                shipForward = math.normalize(shipForward);

                float angle = math.atan2(shipForward.y, shipForward.x);
                quaternion flatRotation = quaternion.RotateZ(angle);

                for (int j = 0; j < hitboxes.Length; j++)
                {
                    var hitbox = hitboxes[j];

                    float3 hitboxWorldCenter = shipWorldPos + math.mul(flatRotation, hitbox.LocalCenter * worldTransform.Value.Scale());
                    quaternion hitboxWorldRotation = math.mul(flatRotation, hitbox.Rotation);

                    float4x4 hitboxWorldToLocal = math.inverse(new float4x4(hitboxWorldRotation, hitboxWorldCenter));

                    float3 halfExtents = hitbox.HalfExtents * worldTransform.Value.Scale();

                    float3 localStartR = math.transform(hitboxWorldToLocal, startR);
                    float3 localEndR = math.transform(hitboxWorldToLocal, endR);

                    float3 localStartL = math.transform(hitboxWorldToLocal, startL);
                    float3 localEndL = math.transform(hitboxWorldToLocal, endL);

                    float3 localStartU = math.transform(hitboxWorldToLocal, startU);
                    float3 localEndU = math.transform(hitboxWorldToLocal, endU);

                    float3 localStartD = math.transform(hitboxWorldToLocal, startD);
                    float3 localEndD = math.transform(hitboxWorldToLocal, endD);

                    if (LineSegmentIntersectsAABB(localStartR, localEndR, halfExtents) ||
                        LineSegmentIntersectsAABB(localStartL, localEndL, halfExtents) ||
                        LineSegmentIntersectsAABB(localStartU, localEndU, halfExtents) ||
                        LineSegmentIntersectsAABB(localStartD, localEndD, halfExtents))
                    {
                        isEnemyHit = true;
                        HitEntity = element.Entity;
                        break;
                    }
                }

                if (isEnemyHit)
                {
                    shouldEarlyExit = true;
                    break;
                }
            }
        }

        private static bool LineSegmentIntersectsAABB(float3 p0, float3 p1, float3 halfExtents)
        {
            float3 m = (p0 + p1) * 0.5f;
            float3 d = p1 - m;

            return math.abs(m.x) <= halfExtents.x + math.abs(d.x) &&
                   math.abs(m.y) <= halfExtents.y + math.abs(d.y);
        }
    }


    [BurstCompile]
    [UpdateInGroup(typeof(CombatCollisionGroup))]
    [UpdateAfter(typeof(BulletMovementSystem))]
    partial struct BulletCollisionSystem : ISystem
    {
        private CachedSpatialDatabaseRO _CachedSpatialDatabase;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SpatialDatabaseSingleton>();
        }


        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.TryGetSingleton<SpatialDatabaseSingleton>(out SpatialDatabaseSingleton spatialDatabaseSingleton))
            {
                _CachedSpatialDatabase = new CachedSpatialDatabaseRO
                {
                    SpatialDatabaseEntity = spatialDatabaseSingleton.TargetablesSpatialDatabase,
                    SpatialDatabaseLookup = SystemAPI.GetComponentLookup<SpatialDatabase>(true),
                    CellsBufferLookup = SystemAPI.GetBufferLookup<SpatialDatabaseCell>(true),
                    ElementsBufferLookup = SystemAPI.GetBufferLookup<SpatialDatabaseElement>(true),
                };

                _CachedSpatialDatabase.CacheData();
            }
            else
            {
                return;
            }


            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (bulletTransform, prevPos, bulletRadius, damage, onHitPrefab, teamTag, bulletEntity)
                     in SystemAPI.Query<RefRO<LocalTransform>, RefRO<PreviousPosition>, RefRO<Radius>, RefRO<Damage>, RefRO<OnHitEffectPrefab>, RefRO<TeamTag>>()
                         .WithAll<BulletTag>()
                         .WithEntityAccess())
            {
                float3 bulletStart = prevPos.ValueRO.Value;
                float3 bulletEnd = bulletTransform.ValueRO.Position;
                float radius = bulletRadius.ValueRO.Value;

                float3 heading = math.normalize(bulletEnd - bulletStart);

                float3 right = new float3(-heading.y, heading.x, 0f);

                float3 up = new float3(0f, 1f, 0f);

                float3 rightOffset = right * (radius / 2f);
                float3 upOffset = up * (radius / 2f);

                float3 startR = bulletStart + rightOffset;
                float3 endR = bulletEnd + rightOffset;

                float3 startL = bulletStart - rightOffset;
                float3 endL = bulletEnd - rightOffset;

                float3 startU = bulletStart + upOffset;
                float3 endU = bulletEnd + upOffset;

                float3 startD = bulletStart - upOffset;
                float3 endD = bulletEnd - upOffset;


                var bulletCollisionDetector = new BulletCollisionDetector(state.EntityManager, teamTag.ValueRO.Team, startR, endR, startL, endL, startU, endU, startD, endD);
                SpatialDatabase.QueryAABB(_CachedSpatialDatabase._SpatialDatabase, _CachedSpatialDatabase._SpatialDatabaseCells, _CachedSpatialDatabase._SpatialDatabaseElements, bulletEnd, new float3(1.0f, 1.0f, 1.0f), ref bulletCollisionDetector);

                if (bulletCollisionDetector.isEnemyHit)
                {
                    var health = SystemAPI.GetComponentRW<Health>(bulletCollisionDetector.HitEntity);
                    health.ValueRW.Value -= damage.ValueRO.Value;

                    var impactParticleRequest = ecb.CreateEntity();

                    ecb.AddComponent(impactParticleRequest, new ImpactSpawnRequest()
                    {
                        Count = 10,
                        Normal = math.up(),
                        Position = bulletEnd,
                        Prefab = onHitPrefab.ValueRO.Value,
                        Scale = 3.0f
                    }); 


                    ecb.DestroyEntity(bulletEntity);
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

    }
}
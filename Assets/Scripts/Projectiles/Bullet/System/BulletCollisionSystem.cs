﻿using SpaceGame.Combat.Components;
using SpaceGame.Movement.Components;
using SpaceGame.SpatialGrid.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SpaceGame.Combat.Systems
{
    [BurstCompile]
    public struct BulletCollisionDetector : ISpatialQueryCollector
    {
        public bool isEnemyHit;
        public Entity HitEntity;

        private int team;
        private EntityManager em;

        private float3 bulletStart;
        private float3 bulletEnd;
        private float radius;

        public BulletCollisionDetector(EntityManager manager, int team, float3 bulletStart, float3 bulletEnd, float radius) : this()
        {
            this.team = team;
            this.em = manager;
            this.bulletEnd = bulletEnd;
            this.bulletStart = bulletStart;
            this.radius = radius;
        }

        [BurstCompile]
        public void OnVisitCell(in SpatialDatabaseCell cell, in UnsafeList<SpatialDatabaseElement> elements, out bool shouldEarlyExit)
        {
            shouldEarlyExit = false;

            for (int i = cell.StartIndex; i < cell.StartIndex + cell.ElementsCount; i++)
            {
                var element = elements[i];

                if (element.Entity == Entity.Null || !em.Exists(element.Entity))
                    continue;

                if (em.HasComponent<TeamTag>(element.Entity))
                {
                    var targetTeam = em.GetComponentData<TeamTag>(element.Entity);
                    if (targetTeam.Team == team)
                        continue;
                }
                var worldTransform = em.GetComponentData<LocalToWorld>(element.Entity);
                var boundingRadius = em.GetComponentData<BoundingRadius>(element.Entity);
                var scaledRadius = boundingRadius.Value;

                var distanceToLineSeg = DistancePointToSegment(worldTransform.Position, bulletStart, bulletEnd);

                if (distanceToLineSeg > scaledRadius + radius)
                    continue;

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

                var shipTransform = em.GetComponentData<LocalTransform>(element.Entity);
                var hitboxes = em.GetBuffer<HitBoxElement>(element.Entity);

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
        [BurstCompile]
        private float DistancePointToSegment(float3 point, float3 a, float3 b)
        {
            float3 ab = b - a;
            float3 ap = point - a;

            float t = math.saturate(math.dot(ap, ab) / math.dot(ab, ab));
            float3 closest = a + t * ab;
            return math.distance(point, closest);
        }

        [BurstCompile]
        private bool LineSegmentIntersectsAABB(float3 p0, float3 p1, float3 halfExtents)
        {
            float3 m = (p0 + p1) * 0.5f;
            float3 d = p1 - m;

            return math.abs(m.x) <= halfExtents.x + math.abs(d.x) &&
                   math.abs(m.y) <= halfExtents.y + math.abs(d.y);
        }
    }


    [BurstCompile]
    [UpdateInGroup(typeof(CombatCollisionGroup))]
    partial struct BulletCollisionSystem : ISystem
    {
        private NativeHashSet<Entity> hitEntities;

        private ComponentLookup<SpatialDatabase> databaseLookup;
        private BufferLookup<SpatialDatabaseCell> cellbufferLookup;
        private BufferLookup<SpatialDatabaseElement> elementLookup;

        public void OnCreate(ref SystemState state)
        {
            databaseLookup = SystemAPI.GetComponentLookup<SpatialDatabase>(true);
            cellbufferLookup = SystemAPI.GetBufferLookup<SpatialDatabaseCell>(true);
            elementLookup = SystemAPI.GetBufferLookup<SpatialDatabaseElement>(true);
            
            hitEntities = new NativeHashSet<Entity>(256, Allocator.Persistent);
            state.RequireForUpdate<SpatialDatabaseSingleton>();
        } 


        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            databaseLookup.Update(ref state);
            cellbufferLookup.Update(ref state);
            elementLookup.Update(ref state);


            if (!SystemAPI.TryGetSingleton<SpatialDatabaseSingleton>(out SpatialDatabaseSingleton spatialDatabaseSingleton))
                return;
            ref var databases = ref spatialDatabaseSingleton.TeamBasedDatabases.Value.TeamBasedDatabases;

            NativeList<CachedSpatialDatabaseRO> databaseList = TeamBasedSpatialDatabaseUtility.ConstructCachedSpatialDatabseROList(ref databases, databaseLookup, cellbufferLookup, elementLookup);

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            hitEntities.Clear();

            foreach (var (bulletTransform, prevPos, bulletRadius, damage, teamTag, bulletId, bulletEntity)
                     in SystemAPI.Query<RefRO<LocalTransform>, RefRO<PreviousPosition>, RefRO<Radius>, RefRO<Damage>, RefRO<TeamTag>, RefRO<ProjectileId>>()
                         .WithAll<BulletTag>()
                         .WithEntityAccess())
            {
                float3 bulletStart = prevPos.ValueRO.Value;
                float3 bulletEnd = bulletTransform.ValueRO.Position;
                float radius = bulletRadius.ValueRO.Value;



                var bulletCollisionDetector = new BulletCollisionDetector(state.EntityManager, teamTag.ValueRO.Team, bulletStart, bulletEnd, radius);
                TeamFilterMode filterMode = TeamFilterMode.DifferentTeam;

                TeamBasedSpatialDatabaseUtility.GetTeamBasedDatabase(databaseList, teamTag.ValueRO.Team, filterMode, out bool found, out CachedSpatialDatabaseRO cachedDb);

                if (!found)
                    continue;

                SpatialDatabase.QueryAABB(cachedDb._SpatialDatabase, cachedDb._SpatialDatabaseCells, cachedDb._SpatialDatabaseElements, bulletEnd, new float3(1.0f, 1.0f, 1.0f), ref bulletCollisionDetector);

                if (bulletCollisionDetector.isEnemyHit)
                {
                    var damageRequestBuffer = SystemAPI.GetBuffer<DamageHealthRequestBuffer>(bulletCollisionDetector.HitEntity);
                    damageRequestBuffer.Add(new DamageHealthRequestBuffer()
                    {
                        Source = bulletEntity,
                        Value = damage.ValueRO.Value
                    });

                    if (!state.EntityManager.HasComponent<NeedHealthUpdateTag>(bulletCollisionDetector.HitEntity))
                    {
                        if (hitEntities.Count >= hitEntities.Capacity - 1)
                            hitEntities.Capacity *= 2;

                        hitEntities.Add(bulletCollisionDetector.HitEntity);
                    }

                    if(SystemAPI.TryGetSingletonBuffer<ImpactSpawnRequest>(out var buffer))
                    {
                        buffer.Add(new ImpactSpawnRequest()
                        {
                            Count = 1,
                            Normal = math.up(),
                            Position = bulletEnd,
                            Scale = radius * 3.0f,
                            PrefabId = bulletId.ValueRO.Value
                        });
                    }


                    if(SystemAPI.TryGetSingletonBuffer<ProjectilePoolRequest>(out var poolBuffer))
                    {
                        poolBuffer.Add(new ProjectilePoolRequest()
                        {
                            Entity = bulletEntity,
                            Id = bulletId.ValueRO.Value
                        });
                    }
                }
            }

            foreach (var entity in hitEntities)
                ecb.AddComponent<NeedHealthUpdateTag>(entity);

            ecb.Playback(state.EntityManager);
            ecb.Dispose();

            foreach (var db in databaseList)
                db.Dispose();

            databaseList.Dispose();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            hitEntities.Dispose();
        }

    }
}
using SpaceGame.Combat.Components;
using SpaceGame.SpatialGrid.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SpaceGame.Combat.Missiles.System
{
    [UpdateInGroup(typeof(CombatCollisionGroup))]
    public partial struct MissileCollisionSystem : ISystem
    { 
        private CachedSpatialDatabaseRO cachedDB;
        private ComponentLookup<SpatialDatabase> databaseLookup;
        private BufferLookup<SpatialDatabaseCell> databaseCellLookup;
        private BufferLookup<SpatialDatabaseElement> elementLookup;

        private BufferLookup<HitBoxElement> HitboxLookup;
        private ComponentLookup<BoundingRadius> RadiusLookup;
        private ComponentLookup<LocalToWorld> LtwLookup;
        private ComponentLookup<NeedHealthUpdateTag> HealthUpdateLookup;
        private BufferLookup<DamageHealthRequestBuffer> DamageBufferLookup;

        private NativeHashSet<Entity> enemiesHitWithoutHealthUpdateTag;
        private NativeList<Entity> enemiesHitThisPass;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            enemiesHitWithoutHealthUpdateTag = new NativeHashSet<Entity>(256, Allocator.Persistent);
            enemiesHitThisPass = new NativeList<Entity>(128, Allocator.Persistent);

            databaseLookup = state.GetComponentLookup<SpatialDatabase>(true);
            databaseCellLookup = state.GetBufferLookup<SpatialDatabaseCell>(true);
            elementLookup = state.GetBufferLookup<SpatialDatabaseElement>(true);

            RadiusLookup = state.GetComponentLookup<BoundingRadius>(true);
            LtwLookup = state.GetComponentLookup<LocalToWorld>(true);
            HitboxLookup = state.GetBufferLookup<HitBoxElement>(true);

            DamageBufferLookup = state.GetBufferLookup<DamageHealthRequestBuffer>(false);
            HealthUpdateLookup = state.GetComponentLookup<NeedHealthUpdateTag>(false);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.TryGetSingleton<SpatialDatabaseSingleton>(out SpatialDatabaseSingleton spatialDatabaseSingleton))
            {
                databaseLookup.Update(ref state);
                databaseCellLookup.Update(ref state);
                elementLookup.Update(ref state);

                cachedDB = new CachedSpatialDatabaseRO
                {
                    SpatialDatabaseEntity = spatialDatabaseSingleton.TargetablesSpatialDatabase,
                    SpatialDatabaseLookup = databaseLookup,
                    CellsBufferLookup = databaseCellLookup,
                    ElementsBufferLookup = elementLookup
                };

                cachedDB.CacheData();
            }
            else
            {
                return;
            }

            RadiusLookup.Update(ref state);
            LtwLookup.Update(ref state);
            HitboxLookup.Update(ref state);
            DamageBufferLookup.Update(ref state);
            HealthUpdateLookup.Update(ref state);

            enemiesHitWithoutHealthUpdateTag.Clear();

            foreach (var (hitboxes, boundingRadius, explosionRadius, damage, teamTag, ltw, projectileId, entity) in SystemAPI.Query<DynamicBuffer<HitBoxElement>,RefRO<BoundingRadius>, RefRO<ExplosionRadius>, RefRO<Damage>,
                RefRO<TeamTag>, RefRO<LocalToWorld>, RefRO<ProjectileId>>().WithEntityAccess())
            {
                enemiesHitThisPass.Clear();
                int team = teamTag.ValueRO.Team;

                CollisionBasedCollector hitCollector = new CollisionBasedCollector(RadiusLookup, HitboxLookup, LtwLookup, team, ltw.ValueRO, hitboxes, boundingRadius.ValueRO.Value);

                SpatialDatabase.QuerySphereCellProximityOrder(cachedDB._SpatialDatabase, cachedDB._SpatialDatabaseCells, cachedDB._SpatialDatabaseElements, ltw.ValueRO.Position, boundingRadius.ValueRO.Value,ref hitCollector);

                if (!hitCollector.Hit)
                    continue;

                var explosionCollector = new RangeBasedTargetingCollectorMultiple(ref enemiesHitThisPass, state.EntityManager, ltw.ValueRO.Position, explosionRadius.ValueRO.Value, TargetFilterMode.DifferentTeam, team);

                SpatialDatabase.QuerySphereCellProximityOrder(cachedDB._SpatialDatabase, cachedDB._SpatialDatabaseCells, cachedDB._SpatialDatabaseElements, ltw.ValueRO.Position, explosionRadius.ValueRO.Value, ref explosionCollector);

                foreach(var hitEntity in explosionCollector.collectedEnemies)
                {
                    if (!DamageBufferLookup.HasBuffer(hitEntity))
                        continue;

                    var targetPos = LtwLookup[hitEntity];
                    var distSq = math.distancesq(targetPos.Position, ltw.ValueRO.Position);

                    var t = 1.0f - math.unlerp(0.0f, explosionRadius.ValueRO.Value * explosionRadius.ValueRO.Value, distSq);
                    var finalDamage = math.lerp(0.0f, damage.ValueRO.Value, t);
                    
                    DamageBufferLookup[hitEntity].Add(new DamageHealthRequestBuffer()
                    {
                        Source = entity,
                        Value = finalDamage
                    });

                    if (!HealthUpdateLookup.HasComponent(hitEntity))
                    {
                        if (enemiesHitWithoutHealthUpdateTag.Count >= enemiesHitWithoutHealthUpdateTag.Capacity - 1)
                            enemiesHitWithoutHealthUpdateTag.Capacity *= 2;

                        enemiesHitWithoutHealthUpdateTag.Add(hitEntity);
                    }
                }

                if (SystemAPI.TryGetSingletonBuffer<ImpactSpawnRequest>(out var buffer))
                {
                    buffer.Add(new ImpactSpawnRequest()
                    {
                        Count = 1,
                        Normal = math.up(),
                        Position = ltw.ValueRO.Position,
                        PrefabId = projectileId.ValueRO.Value,
                        Scale = explosionRadius.ValueRO.Value / 2.0f
                    }); 
                }

                if (SystemAPI.TryGetSingletonBuffer<ProjectilePoolRequest>(out var poolBuffer))
                {
                    poolBuffer.Add(new ProjectilePoolRequest()
                    {
                        Entity = entity,
                        Id = projectileId.ValueRO.Value
                    });
                }
            }

            foreach (var entity in enemiesHitWithoutHealthUpdateTag)
                state.EntityManager.AddComponent<NeedHealthUpdateTag>(entity);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            enemiesHitWithoutHealthUpdateTag.Dispose();
            enemiesHitThisPass.Dispose();
        }
    }
}

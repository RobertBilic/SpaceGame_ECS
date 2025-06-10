using SpaceGame.Combat.Components;
using SpaceGame.Combat.StateTransition.Components;
using SpaceGame.Detection.Component;
using SpaceGame.SpatialGrid.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SpaceGame.Combat.Fleets
{
    [UpdateInGroup(typeof(CombatStateTransitionGroup))]
    public partial struct FormFleetTransitionSystem : ISystem
    {
        private CachedSpatialDatabaseRO _CachedSpatialDatabase;
        private NativeList<Entity> collectedEntities;
        private Random random;
        private float checkPeriod;
        private float timeSinceLastCheck;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            timeSinceLastCheck = float.MaxValue;
            checkPeriod = 5.0f;

            collectedEntities = new NativeList<Entity>(Allocator.Persistent);
            random = new Random(33221144);
            state.RequireForUpdate<NeedsCombatStateChange>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<GlobalTimeComponent>(out var timeComp))
                return;

            timeSinceLastCheck += timeComp.DeltaTimeScaled;

            if (timeSinceLastCheck < checkPeriod)
                return;

            timeSinceLastCheck = 0.0f;

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

            foreach (var (ltw, detectionRange, teamTag, stateWeights, stateSpecificComp, entity) in SystemAPI.Query<RefRO<LocalToWorld>,RefRO<DetectionRange>, RefRO<TeamTag>,
                 DynamicBuffer<CombatStateChangeWeight>, DynamicBuffer<NewCombatStateSpecificComponent>>()
                .WithNone<FleetMember, DetectedEntity, FleetLeader>()
                .WithNone<FormFleetTag>()
                .WithAll<CombatEntity, NeedsCombatStateChange>()
                .WithEntityAccess())
            {
                collectedEntities.Clear();

                RangeBasedTargetingCollectorMultiple queryCollector = new RangeBasedTargetingCollectorMultiple(ref collectedEntities, state.EntityManager
                    , ltw.ValueRO.Position, detectionRange.ValueRO.Value, TargetFilterMode.SameTeam, teamTag.ValueRO.Team);

                SpatialDatabase.QuerySphereCellProximityOrder(_CachedSpatialDatabase._SpatialDatabase, _CachedSpatialDatabase._SpatialDatabaseCells, _CachedSpatialDatabase._SpatialDatabaseElements,
                    ltw.ValueRO.Position, detectionRange.ValueRO.Value, ref queryCollector);

                //TODO: Fleet archetypes, this is an arbitrary number atm

                int validEntities = 0;
                
                for(int i = 0; i < collectedEntities.Length; i++)
                {
                    if (state.EntityManager.HasComponent<FleetMember>(collectedEntities[i]) || state.EntityManager.HasComponent<FleetLeader>(collectedEntities[i]))
                        continue;

                    validEntities++;
                }


                if (validEntities < 5)
                    continue;

                var behaviorTag = ComponentType.ReadOnly<FormFleetTag>();
                var weight = random.NextFloat(40, 80);
                
                stateWeights.Add(new CombatStateChangeWeight()
                {
                    BehaviourTag = behaviorTag,
                    Weight = weight
                });

                stateSpecificComp.Add(new NewCombatStateSpecificComponent()
                {
                    Tag = behaviorTag,
                    Value = ComponentType.ReadOnly<FormFleetTag>()
                });
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            collectedEntities.Dispose();
        }
    }
}

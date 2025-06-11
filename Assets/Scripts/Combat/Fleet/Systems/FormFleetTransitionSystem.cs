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
        private NativeList<Entity> collectedEntities;
        private Random random;
        private float checkPeriod;
        private float timeSinceLastCheck;

        ComponentLookup<SpatialDatabase> spatialDatabaseLookup;
        BufferLookup<SpatialDatabaseElement> spatialDatabaseElementLookup;
        BufferLookup<SpatialDatabaseCell> spatialDatabaseCellLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            timeSinceLastCheck = float.MaxValue;
            checkPeriod = 5.0f;

            spatialDatabaseLookup = state.GetComponentLookup<SpatialDatabase>(true);
            spatialDatabaseElementLookup = state.GetBufferLookup<SpatialDatabaseElement>(true);
            spatialDatabaseCellLookup = state.GetBufferLookup<SpatialDatabaseCell>(true);

            collectedEntities = new NativeList<Entity>(Allocator.Persistent);
            random = new Random(33221144);
            state.RequireForUpdate<NeedsCombatStateChange>();
            state.RequireForUpdate<EnableDynamicFleets>();
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

            spatialDatabaseLookup.Update(ref state);
            spatialDatabaseElementLookup.Update(ref state);
            spatialDatabaseCellLookup.Update(ref state);

            if (!SystemAPI.TryGetSingleton<SpatialDatabaseSingleton>(out SpatialDatabaseSingleton spatialDatabaseSingleton))
                return;

            var databases = TeamBasedSpatialDatabaseUtility.ConstructCachedSpatialDatabseROList(spatialDatabaseSingleton, spatialDatabaseLookup, spatialDatabaseCellLookup, spatialDatabaseElementLookup);

            foreach (var (ltw, detectionRange, teamTag, stateWeights, stateSpecificComp, entity) in SystemAPI.Query<RefRO<LocalToWorld>,RefRO<DetectionRange>, RefRO<TeamTag>,
                 DynamicBuffer<CombatStateChangeWeight>, DynamicBuffer<NewCombatStateSpecificComponent>>()
                .WithNone<FleetMember, DetectedEntity, FleetLeader>()
                .WithNone<FormFleetTag>()
                .WithAll<CombatEntity, NeedsCombatStateChange>()
                .WithEntityAccess())
            {
                collectedEntities.Clear();

                RangeBasedTargetingCollectorMultiple queryCollector = new RangeBasedTargetingCollectorMultiple(ref collectedEntities, state.EntityManager
                    , ltw.ValueRO.Position, detectionRange.ValueRO.Value, TeamFilterMode.SameTeam, teamTag.ValueRO.Team);

                TeamBasedSpatialDatabaseUtility.GetTeamBasedDatabase(databases, teamTag.ValueRO.Team, TeamFilterMode.SameTeam, out var found, out var cachedDb);

                if (!found)
                    continue;

                SpatialDatabase.QuerySphereCellProximityOrder(cachedDb._SpatialDatabase, cachedDb._SpatialDatabaseCells, cachedDb._SpatialDatabaseElements,
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

            foreach (var db in databases)
                db.Dispose();

            databases.Dispose();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            collectedEntities.Dispose();
        }
    }
}

using Unity.Burst;
using Unity.Entities;
using SpaceGame.Combat.Patrol.Components;
using SpaceGame.Combat.StateTransition.Components;
using Unity.Mathematics;
using SpaceGame.Movement.Components;

namespace SpaceGame.Combat.Patrol.Systems
{
    partial struct PatrolTransitionSystem : ISystem
    {
        Random rnd;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SupportsPatrolTag>();
            rnd = new Random(333222111);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (stateWeights, stateSpecificComponents, entity) in SystemAPI.Query<DynamicBuffer<CombatStateChangeWeight>, DynamicBuffer<NewCombatStateSpecificComponent>>()
                .WithAll<SupportsPatrolTag, NeedsCombatStateChange>()
                .WithNone<DetectedEntity, Controllable>()
                .WithEntityAccess())
            {
                float minBound = 0.0f;
                var behaviourTag = ComponentType.ReadOnly<PatrolTag>();

                if (state.EntityManager.HasBuffer<PatrolWaypoint>(entity))
                {
                    //If the player has a persistant patrol buffer there are increased chances to return to the patrol after combat
                    minBound = 50.0f;
                }

                stateSpecificComponents.Add(new NewCombatStateSpecificComponent()
                {
                    Tag = behaviourTag,
                    Value = ComponentType.ReadOnly<NeedsPatrolInitializationTag>()
                });
                stateSpecificComponents.Add(new NewCombatStateSpecificComponent()
                {
                    Tag = behaviourTag,
                    Value = ComponentType.ReadOnly<PatrolWaypointIndex>()
                });
                stateSpecificComponents.Add(new NewCombatStateSpecificComponent()
                {
                    Tag = behaviourTag,
                    Value = ComponentType.ReadOnly<DesiredSpeed>()
                });
                stateSpecificComponents.Add(new NewCombatStateSpecificComponent()
                {
                    Tag = behaviourTag,
                    Value = ComponentType.ReadOnly<DesiredMovementDirection>()
                });

                stateWeights.Add(new CombatStateChangeWeight()
                {
                    BehaviourTag = behaviourTag,
                    Weight = rnd.NextFloat(minBound, 100.0f)
                });
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}
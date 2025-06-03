using SpaceGame.Combat.StateTransition.Components;
using SpaceGame.Movement.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace SpaceGame.Combat.StateTransition.Systems
{
    [UpdateInGroup(typeof(CombatStateTransitionGroup))]
    partial struct DogfightTransitionSystem : ISystem
    {
        private const float MaxWeight = 100.0f;

        ComponentLookup<ThrustSettings> moveSpeedLookup;
        ComponentLookup<RotationSpeed> rotationSpeedLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            moveSpeedLookup = state.GetComponentLookup<ThrustSettings>(true);
            rotationSpeedLookup = state.GetComponentLookup<RotationSpeed>(true);

            state.RequireForUpdate<SupportsDogfightTag>();
            state.RequireForUpdate<NeedsCombatStateChange>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            moveSpeedLookup.Update(ref state);
            rotationSpeedLookup.Update(ref state);

            foreach(var (detectedEntity, stateWeights, stateSpecificComponents, entity) in SystemAPI.Query<RefRO<DetectedEntity>, DynamicBuffer<CombatStateChangeWeight>, DynamicBuffer<NewCombatStateSpecificComponent>>()
                .WithAll<SupportsDogfightTag, NeedsCombatStateChange>()
                .WithEntityAccess())
            {
                var detected = detectedEntity.ValueRO.Value;

                if (detected == Entity.Null || !state.EntityManager.Exists(detected))
                    continue;

                float mySpeed = 0.0f;
                float myRotationSpeed = 0.0f;

                float targetSpeed = 0.0f;
                float targetRotationSpeed = 0.0f;

                float rotationSpeedMaxWeight = MaxWeight * 0.6f;
                float moveSpeedMaxWeight = MaxWeight * 0.4f;

                if (moveSpeedLookup.HasComponent(entity))
                    mySpeed = moveSpeedLookup[entity].MaxSpeed;
                if (moveSpeedLookup.HasComponent(detected))
                    targetSpeed = moveSpeedLookup[detected].MaxSpeed;

                if (rotationSpeedLookup.HasComponent(entity))
                    myRotationSpeed = rotationSpeedLookup[entity].Value;
                if (rotationSpeedLookup.HasComponent(detected))
                    targetRotationSpeed = rotationSpeedLookup[detected].Value;


                var rotationT = 1.0f - math.unlerp(myRotationSpeed / 2.0f, myRotationSpeed * 2.0f, targetRotationSpeed);
                rotationT = math.clamp(rotationT, 0.0f, 1.0f);
                var rotationWeight = math.lerp(0.0f, rotationSpeedMaxWeight, rotationT);

                var speedT = 1.0f - math.unlerp(mySpeed / 1.5f, mySpeed * 1.5f, targetSpeed);
                speedT = math.clamp(speedT, 0.0f, 1.0f);
                var speedWeight = math.lerp(0.0f, moveSpeedMaxWeight, speedT);

                var tag = ComponentType.ReadOnly<DogfightTag>();

                stateWeights.Add(new CombatStateChangeWeight()
                {
                    BehaviourTag = tag,
                    Weight = speedWeight + rotationWeight
                });
                
                stateSpecificComponents.Add(new NewCombatStateSpecificComponent()
                {
                    Tag = tag,
                    Value = ComponentType.ReadOnly<DogfightStateComponent>()
                });

                stateSpecificComponents.Add(new NewCombatStateSpecificComponent()
                {
                    Tag = tag,
                    Value = ComponentType.ReadOnly<DisengageCurveDirection>()
                });

                stateSpecificComponents.Add(new NewCombatStateSpecificComponent()
                {
                    Tag = tag,
                    Value = ComponentType.ReadOnly<DisengageSide>()
                });
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}
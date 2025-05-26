using SpaceGame.Combat.StateTransition.Components;
using Unity.Burst;
using Unity.Entities;

namespace SpaceGame.Combat.StateTransition.Systems
{
    [UpdateInGroup(typeof(CombatStateTransitionGroup), OrderLast = true)]
    partial struct CombatStateTransitionSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NeedsCombatStateChange>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            foreach(var (stateWeights, newCombatStateSpecificComponents,entity) in SystemAPI.Query<DynamicBuffer<CombatStateChangeWeight>, DynamicBuffer<NewCombatStateSpecificComponent>>()
                .WithAll<NeedsCombatStateChange>()
                .WithEntityAccess())
            {
                float maxWeight = float.MinValue;
                ComponentType selectedBehavior = default(ComponentType);

                for(int i = 0; i < stateWeights.Length; i++)
                {
                    if(maxWeight < stateWeights[i].Weight)
                    {
                        maxWeight = stateWeights[i].Weight;
                        selectedBehavior = stateWeights[i].BehaviourTag;
                    }
                }

                stateWeights.Clear();

                if(selectedBehavior == default(ComponentType))
                {
                    UnityEngine.Debug.LogWarning($"Entity {entity.Index} marked as needed state change, but doesn't have weights for states");
                    continue;
                }

                if(state.EntityManager.HasComponent<CurrentCombatBehaviour>(entity))
                {
                    var oldBehaviour = state.EntityManager.GetComponentData<CurrentCombatBehaviour>(entity);
                    var oldComponents = state.EntityManager.GetBuffer<ExistingCombatStateSpecificComponent>(entity);

                    ecb.RemoveComponent(entity, oldBehaviour.Value);
                    oldBehaviour.Value = selectedBehavior;
                    ecb.SetComponent(entity, oldBehaviour);

                    foreach(var existingComp in oldComponents)
                    {
                        bool exists = false;

                        foreach(var newComp in newCombatStateSpecificComponents)
                        {
                            if (newComp.Tag != selectedBehavior)
                                continue;

                            if(newComp.Value == existingComp.Value)
                            {
                                exists = true;
                                break;
                            }    
                        }

                        if (!exists)
                            ecb.RemoveComponent(entity, existingComp.Value);
                    }

                    oldComponents.Clear();

                    foreach (var comp in newCombatStateSpecificComponents)
                    {
                        if (comp.Tag != selectedBehavior)
                            continue;

                        oldComponents.Add(new ExistingCombatStateSpecificComponent() { Value = comp.Value });
                        ecb.AddComponent(entity, comp.Value);
                    }

                    newCombatStateSpecificComponents.Clear();
                }
                else
                {
                    ecb.AddComponent(entity, new CurrentCombatBehaviour() { Value = selectedBehavior });
                    var buffer = state.EntityManager.GetBuffer<ExistingCombatStateSpecificComponent>(entity);

                    foreach (var comp in newCombatStateSpecificComponents)
                    {
                        if (comp.Tag != selectedBehavior)
                            continue;

                        buffer.Add(new ExistingCombatStateSpecificComponent() { Value = comp.Value });
                        ecb.AddComponent(entity, comp.Value);
                    }
                    newCombatStateSpecificComponents.Clear();
                }

                ecb.AddComponent(entity, selectedBehavior);
                ecb.RemoveComponent<NeedsCombatStateChange>(entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}
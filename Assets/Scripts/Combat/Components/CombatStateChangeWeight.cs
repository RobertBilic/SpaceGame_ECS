using Unity.Entities;

namespace SpaceGame.Combat.StateTransition.Components
{
    [InternalBufferCapacity(8)]
    public struct CombatStateChangeWeight : IBufferElementData
    {
        public ComponentType BehaviourTag;
        public float Weight;
    }

    public struct CurrentCombatBehaviour : IComponentData
    {
        public ComponentType Value;
    }
}
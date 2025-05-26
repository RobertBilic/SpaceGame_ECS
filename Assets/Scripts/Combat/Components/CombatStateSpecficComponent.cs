using Unity.Entities;

namespace SpaceGame.Combat.StateTransition.Components
{
    [InternalBufferCapacity(8)]
    public struct ExistingCombatStateSpecificComponent : IBufferElementData
    {
        public ComponentType Value;
    }

    [InternalBufferCapacity(32)]
    public struct NewCombatStateSpecificComponent : IBufferElementData
    {
        public ComponentType Value;
        public ComponentType Tag;
    }
}


using Unity.Entities;

namespace SpaceGame.Combat.Components
{
    public struct TimeScaleChangeRequest : IComponentData
    {
        public float Value;
    }
}

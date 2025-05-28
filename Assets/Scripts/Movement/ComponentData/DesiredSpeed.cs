using Unity.Entities;

namespace SpaceGame.Movement.Components
{
    public struct DesiredSpeed : IComponentData
    {
        public float Value;
    }
}
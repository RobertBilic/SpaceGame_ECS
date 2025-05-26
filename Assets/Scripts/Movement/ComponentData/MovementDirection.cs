using Unity.Entities;
using Unity.Mathematics;

namespace SpaceGame.Movement.Components
{
    public struct MovementDirection : IComponentData
    {
        public float3 Value;
    }

    public struct DesiredMovementDirection : IComponentData
    {
        public float3 Value;
    }
}

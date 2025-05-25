using Unity.Entities;
using Unity.Mathematics;

namespace SpaceGame.Movement.Components
{
    public struct MovementTarget : IComponentData
    {
        public Entity TargetEntity;
        public float3 Value;
    }
}

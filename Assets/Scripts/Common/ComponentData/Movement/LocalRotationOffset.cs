using Unity.Entities;
using Unity.Mathematics;

namespace SpaceGame.Movement.Components
{
    public struct LocalRotationOffset : IComponentData
    {
        public float3 Value;
    }
}
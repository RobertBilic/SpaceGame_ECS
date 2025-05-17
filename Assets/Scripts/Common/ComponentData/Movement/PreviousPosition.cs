using Unity.Entities;
using Unity.Mathematics;

namespace SpaceGame.Movement.Components
{
    public struct PreviousPosition : IComponentData
    {
        public float3 Value;
    }
}
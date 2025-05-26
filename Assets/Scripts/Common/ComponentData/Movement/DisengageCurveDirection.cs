using Unity.Entities;
using Unity.Mathematics;

namespace SpaceGame.Movement.Components
{
    public struct DisengageCurveDirection : IComponentData
    {
        public float3 Direction;
    }
}
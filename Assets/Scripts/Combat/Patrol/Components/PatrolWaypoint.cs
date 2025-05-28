using Unity.Entities;
using Unity.Mathematics;

namespace SpaceGame.Combat.Patrol.Components
{
    public struct PatrolWaypoint : IBufferElementData
    {
        public float3 Value;
    }
}
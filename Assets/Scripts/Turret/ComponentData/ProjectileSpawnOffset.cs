using Unity.Entities;
using Unity.Mathematics;

namespace SpaceGame.Combat.Components
{
    public struct ProjectileSpawnOffset : IBufferElementData
    {
        public float3 Value;
    }
}
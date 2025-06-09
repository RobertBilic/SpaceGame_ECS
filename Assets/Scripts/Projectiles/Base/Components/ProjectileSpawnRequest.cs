using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace SpaceGame.Combat.Components
{
    [InternalBufferCapacity(128)]
    public struct ProjectileSpawnRequest : IBufferElementData
    {
        public Entity Target;

        public FixedString32Bytes BulletId;
        public float3 Heading;
        public float3 Position;
        public int Team;

        public float ParentScale;
        public float Range;
        public float Damage;
    }

    [InternalBufferCapacity(128)]
    public struct ProjectilePoolRequest : IBufferElementData
    {
        public Entity Entity;
        public FixedString32Bytes Id;
    }
}
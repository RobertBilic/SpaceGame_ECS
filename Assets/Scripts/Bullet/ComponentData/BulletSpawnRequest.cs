using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[InternalBufferCapacity(128)]
public struct BulletSpawnRequest : IBufferElementData
{
    public FixedString32Bytes BulletId;
    public float3 Heading;
    public float3 Position;
    public int Team;

    public float Range;
    public float Damage;
}

[InternalBufferCapacity(128)]
public struct BulletPoolRequest : IBufferElementData
{
    public Entity Entity;
    public FixedString32Bytes Id;
}

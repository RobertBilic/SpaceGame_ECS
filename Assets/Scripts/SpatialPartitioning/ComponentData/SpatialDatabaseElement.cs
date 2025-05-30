using Unity.Entities;
using Unity.Mathematics;

[InternalBufferCapacity(0)]
public struct SpatialDatabaseElement : IBufferElementData
{
    public Entity Entity;
    public float3 Position;
    public byte Team;
}
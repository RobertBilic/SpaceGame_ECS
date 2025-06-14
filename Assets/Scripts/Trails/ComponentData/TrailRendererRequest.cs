using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct TrailRendererRequest : IBufferElementData
{
    public Entity Entity;
    public FixedString32Bytes TrailId;
    public float3 Position;
    public float3 Forward;
}

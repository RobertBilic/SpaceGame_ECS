using Unity.Entities;
using Unity.Mathematics;

public struct HitBoxElement : IBufferElementData
{
    public float3 LocalCenter;
    public float3 HalfExtents;
    public quaternion Rotation;
}

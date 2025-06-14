using Unity.Entities;
using Unity.Mathematics;

public struct CameraData : IComponentData
{
    public float3 Position;
    public float Aspect;
    public float OrtographicSize;
}

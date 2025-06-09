using Unity.Entities;
using Unity.Mathematics;

public struct MissileLoadedIndicator : IBufferElementData
{
    public Entity Entity;
    public float3 LoadedPosition;
    public float3 UnloadedPosition;
}

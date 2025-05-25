using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct LevelShipEntry : IBufferElementData
{
    public FixedString32Bytes Id;
    public int Team;
    public int Count;
    public float3 Position;
    public float SpawnRadius;
}

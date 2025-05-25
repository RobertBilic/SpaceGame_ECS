using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct ReinforcementEntry : IBufferElementData
{
    public FixedString32Bytes Id;
    public float3 SpawnPosition;
    public float SpawnRadius;
    public float Delay;
    public int Count;
}

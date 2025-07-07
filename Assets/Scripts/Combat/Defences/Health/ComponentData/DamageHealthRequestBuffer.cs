using SpaceGame;
using Unity.Entities;
using Unity.Mathematics;

[InternalBufferCapacity(16)]
public struct DamageHealthRequestBuffer : IBufferElementData
{
    public float Value;
    public DamageType DamageType;
    public Entity Source;
    public float3 SourcePosition;
}
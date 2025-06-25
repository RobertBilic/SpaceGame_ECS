using SpaceGame;
using Unity.Entities;

[InternalBufferCapacity(16)]
public struct DamageHealthRequestBuffer : IBufferElementData
{
    public float Value;
    public DamageType DamageType;
    public Entity Source;
}
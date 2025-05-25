using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct ShipConstructionRequest : IComponentData
{
    public FixedString32Bytes Id;
    public float3 SpawnPosition;
    public int Team;
}

[InternalBufferCapacity(4)]
public struct ShipConstructionAddonRequest : IBufferElementData
{
    public ComponentType ComponentToAdd;
}

[InternalBufferCapacity(12)]
public struct ShipTurretConstructionRequest : IBufferElementData
{
    public FixedString32Bytes Id;
    public float Scale;
    public float3 Position;
}

[InternalBufferCapacity(12)]
public struct ShipForwardWeaponConstructionRequest : IBufferElementData
{
    public FixedString32Bytes Id;
    public float Scale;
    public float3 Position;
    public float3 OriginalHeading;
}
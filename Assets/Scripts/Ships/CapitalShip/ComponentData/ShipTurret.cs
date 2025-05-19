using Unity.Entities;
using Unity.Mathematics;

public struct ShipTurret : IBufferElementData
{
    public Entity TurretPrefab;
    public float3 Position;
    public float Scale;
}
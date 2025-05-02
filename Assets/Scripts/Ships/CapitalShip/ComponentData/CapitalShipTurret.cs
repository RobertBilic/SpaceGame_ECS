using Unity.Entities;
using Unity.Mathematics;

public struct CapitalShipTurret : IBufferElementData
{
    public Entity TurretPrefab;
    public float3 Position;
    public float Scale;
}
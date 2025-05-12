using Unity.Entities;
using Unity.Mathematics;

public struct CapitalShipConstructionRequest : IComponentData
{
    public Entity CapitalShipPrefab;

    public float MoveSpeed;
    public float RotationSpeed;

    public float3 SpawnPosition;
    public float Scale;
}

using Unity.Entities;

public struct Config : IComponentData
{
    public Entity SpatialDatabasePrefab;
    public int ShipsSpatialDatabaseCellCapacity;
    public int SpatialDatabaseSubdivisions;

    public int GameSize;

    public bool IsInitialized;
}

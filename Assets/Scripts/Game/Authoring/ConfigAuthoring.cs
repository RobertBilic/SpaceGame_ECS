using Unity.Entities;
using UnityEngine;

class ConfigAuthoring : MonoBehaviour
{
    [Header("Spatial Database Settings")]
    public GameObject SpatialDatabasePrefab;
    public int SpatialDatabaseSubdivisions = 5;
    public int ShipsSpatialDatabaseCellCapacity = 256;
}

class ConfigAuthoringBaker : Baker<ConfigAuthoring>
{
    public override void Bake(ConfigAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.None);

        AddComponent(entity, new Config() { 
            SpatialDatabasePrefab = GetEntity(authoring.SpatialDatabasePrefab, TransformUsageFlags.None),
            ShipsSpatialDatabaseCellCapacity = authoring.ShipsSpatialDatabaseCellCapacity,
            SpatialDatabaseSubdivisions = authoring.SpatialDatabaseSubdivisions,
            GameSize = 500,
            IsInitialized = false
        });
    }
}

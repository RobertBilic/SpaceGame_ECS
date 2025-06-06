using Unity.Entities;

public struct ProjectilePrefabLookupSingleton : IComponentData
{
    public BlobAssetReference<ProjectilePrefabLookup> Lookup;
}
using Unity.Entities;

public struct BulletPrefabLookupSingleton : IComponentData
{
    public BlobAssetReference<BulletPrefabLookup> Lookup;
}
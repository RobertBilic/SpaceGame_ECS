using Unity.Collections;
using Unity.Entities;

namespace SpaceGame.Combat.Components
{
    public struct TurretPrefabLookupSingleton : IComponentData
    {
        public BlobAssetReference<TurretPrefabLookup> Lookup;
    }

    public struct TurretPrefabLookup
    {
        public BlobArray<TurretPrefab> Entries;

        public TurretPrefab GetPrefab(FixedString32Bytes id)
        {
            for (int i = 0; i < Entries.Length; i++)
            {
                if (Entries[i].Id.Equals(id))
                    return Entries[i];
            }

            return default(TurretPrefab);
        }
    }
}

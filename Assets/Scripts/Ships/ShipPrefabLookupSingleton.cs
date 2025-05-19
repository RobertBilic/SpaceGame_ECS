using Unity.Collections;
using Unity.Entities;

namespace SpaceGame.Combat.Components
{
    public struct ShipPrefabLookupSingleton : IComponentData
    {
        public BlobAssetReference<ShipPrefabLookup> Lookup;
    }

    public struct ShipPrefabLookup
    {
        public BlobArray<ShipPrefab> Entries;

        public ShipPrefab GetPrefab(FixedString32Bytes id)
        {
            for (int i = 0; i < Entries.Length; i++)
            {
                if (Entries[i].Id.Equals(id))
                    return Entries[i];
            }

            return default(ShipPrefab);
        }
    }
}

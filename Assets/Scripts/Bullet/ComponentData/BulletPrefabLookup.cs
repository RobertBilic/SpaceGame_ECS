using SpaceGame.Combat.Components;
using Unity.Collections;
using Unity.Entities;

public struct BulletPrefabLookup
{
    public BlobArray<BulletPrefab> Entries;

    public BulletPrefab GetPrefab(FixedString32Bytes id)
    {
        for (int i = 0; i < Entries.Length; i++)
        {
            if (Entries[i].Id.Equals(id))
                return Entries[i];
        }

        return default(BulletPrefab);
    }
}
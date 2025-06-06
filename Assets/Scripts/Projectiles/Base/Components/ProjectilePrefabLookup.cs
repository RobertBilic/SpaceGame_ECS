using SpaceGame.Combat.Components;
using Unity.Collections;
using Unity.Entities;

public struct ProjectilePrefabLookup
{
    public BlobArray<ProjectilePrefab> Entries;

    public ProjectilePrefab GetPrefab(FixedString32Bytes id)
    {
        for (int i = 0; i < Entries.Length; i++)
        {
            if (Entries[i].Id.Equals(id))
                return Entries[i];
        }

        return default(ProjectilePrefab);
    }
}


using Unity.Entities;

namespace SpaceGame.SpatialGrid.Components
{
    public struct SpatialDatabaseSingleton : IComponentData
    {
        public Entity AllTargetablesDatabase;
        public Entity TrailDatabase;
        public BlobAssetReference<TeamSpatialDatabaseLookup> TeamBasedDatabases;
    }

    public struct TeamSpatialDatabaseLookup
    {
        public BlobArray<TeamSpatialDatabase> TeamBasedDatabases;

        public TeamSpatialDatabase GetPrefab(int team)
        {
            for (int i = 0; i < TeamBasedDatabases.Length; i++)
            {
                if (TeamBasedDatabases[i].Team == team)
                    return TeamBasedDatabases[i];
            }

            return default(TeamSpatialDatabase);
        }
    }

    public struct TeamSpatialDatabase
    {
        public int Team;
        public Entity Database;
    }
}
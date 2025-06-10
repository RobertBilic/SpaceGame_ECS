using Unity.Entities;

namespace SpaceGame.Game.Initialization.Components
{
    public struct Config : IComponentData
    {
        public Entity SpatialDatabasePrefab;
        public int ShipsSpatialDatabaseCellCapacity;
        public int SpatialDatabaseSubdivisions;

        public int TeamCount;
        public int GameSize;
        public bool IsInitialized;
    }
}
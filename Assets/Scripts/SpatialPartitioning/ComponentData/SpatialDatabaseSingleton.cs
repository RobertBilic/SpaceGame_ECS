using Unity.Entities;

namespace SpaceGame.SpatialGrid.Components
{
    public struct SpatialDatabaseSingleton : IComponentData
    {
        public Entity TargetablesSpatialDatabase;
    }
}
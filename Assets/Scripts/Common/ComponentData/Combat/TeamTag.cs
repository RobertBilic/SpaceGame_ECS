using Unity.Entities;

namespace SpaceGame.Combat.Components
{
    public struct TeamTag : IComponentData
    {
        public int Team;
    }
}
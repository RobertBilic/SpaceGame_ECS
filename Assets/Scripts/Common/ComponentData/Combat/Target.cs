using Unity.Entities;

namespace SpaceGame.Combat.Components
{
    public struct Target : IComponentData
    {
        public Entity Value;
    }
}
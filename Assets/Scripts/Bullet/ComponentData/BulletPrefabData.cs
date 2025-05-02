using Unity.Entities;

namespace SpaceGame.Combat.Components
{
    public struct BulletPrefabData : IComponentData
    {
        public Entity Entity;
        public float BulletSpeed;
        public float Scale;
    }
}
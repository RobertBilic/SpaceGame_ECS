using Unity.Collections;
using Unity.Entities;

namespace SpaceGame.Combat.Components
{
    public struct BulletPrefab : IComponentData
    {
        public FixedString32Bytes Id; 

        public Entity Entity;
        public float Scale;
        public float Speed;

        public Entity OnHitEntity;
    }
}
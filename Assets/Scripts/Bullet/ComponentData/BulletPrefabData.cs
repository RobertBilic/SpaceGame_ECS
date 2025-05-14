using Unity.Collections;
using Unity.Entities;

namespace SpaceGame.Combat.Components
{
    public struct BulletPrefab : IComponentData
    {
        public FixedString32Bytes Id; 
        public Entity Entity;
    }
}
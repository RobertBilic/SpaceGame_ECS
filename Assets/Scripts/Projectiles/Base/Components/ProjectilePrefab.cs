using Unity.Collections;
using Unity.Entities;

namespace SpaceGame.Combat.Components
{
    public struct ProjectilePrefab : IComponentData
    {
        public FixedString32Bytes Id; 
        public Entity Entity;
    }
}
using Unity.Collections;
using Unity.Entities;

namespace SpaceGame.Combat.Components
{
    public struct ProjectileId : IComponentData
    {
        public FixedString32Bytes Value;
    }
}
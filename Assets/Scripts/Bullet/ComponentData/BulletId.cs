using Unity.Collections;
using Unity.Entities;

namespace SpaceGame.Combat.Components
{
    public struct BulletId : IComponentData
    {
        public FixedString32Bytes Value;
    }
}
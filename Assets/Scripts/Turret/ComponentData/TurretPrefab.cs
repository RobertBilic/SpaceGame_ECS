using Unity.Collections;
using Unity.Entities;

namespace SpaceGame.Combat.Components
{
    public struct TurretPrefab : IComponentData
    {
        public Entity PrefabEntity;
        public FixedString32Bytes Id;
    }
}
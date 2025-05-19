using Unity.Collections;
using Unity.Entities;

namespace SpaceGame.Combat.Components
{
    public struct ShipPrefab : IComponentData
    {
        public FixedString32Bytes Id;
        public Entity Value;
    }
}
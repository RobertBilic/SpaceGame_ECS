using Unity.Entities;

namespace SpaceGame.Combat.Components
{
    public struct Damage : IComponentData
    {
        public float Value;
        public DamageType Type;
    }
}
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace SpaceGame.Combat.Components
{
    public struct ForwardWeapon : IComponentData
    {
        public FixedString32Bytes BulletId;

        public float Range;
        public float Damage;
        public DamageType DamageType;
        public float LastFireTime;
        public float FiringRate;
        public Entity RotationBase;

        
        public float MaxRotationPadding;
        public float MaxRotationAngleDeg;     
        public float CurrentRotationOffsetDeg;
        public float RotationSpeed;
    }
}
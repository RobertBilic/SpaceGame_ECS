using SpaceGame.Combat.Components;
using SpaceGame.Movement.Components;
using SpaceGame.SpatialGrid.Components;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace SpaceGame.Combat.Authoring
{
    class ShipAuthoring : MonoWithHitbox
    {
        public float Speed;
        public float RotationSpeed;
        public float ApproachDistance;

        public float MaxShipBankingAngle;
        public float BankingSmoothSpeed;

        public List<ForwardWeaponAuthoring> Weapons;

        [Header("Health")]
        public float Health;
        public GameObject HealthBar;
    }

    class TestEnemyBaker : BakerWithHitboxes<ShipAuthoring>
    {
        protected override void BakeAdditionalData(Entity entity, ShipAuthoring authoring)
        {
            AddComponent(entity, new Health() { Current = authoring.Health, Max = authoring.Health });
            AddComponent(entity, new MoveSpeed() { Value = authoring.Speed });
            AddComponent(entity, new RotationSpeed() { Value = authoring.RotationSpeed });
            AddComponent(entity, new ApproachDistance() { Value = authoring.ApproachDistance });
            AddComponent(entity, new CurrentRotation() { Value = 0.0f });
            AddComponent(entity, new ShipMovementBehaviourState() { Value = ShipMovementBehaviour.MoveToTarget });
            AddComponent(entity, new ShipBankingData()
            {
                CurrentBankAngle = 0,
                MaxBankAngle = authoring.MaxShipBankingAngle,
                SmoothSpeed = authoring.BankingSmoothSpeed
            });
            AddComponent(entity, new Target());
            
            if (authoring.Weapons.Count != 0)
            {
                var weaponBuffer = AddBuffer<ForwardWeaponElement>(entity);

                foreach (var weapon in authoring.Weapons)
                    weaponBuffer.Add(new ForwardWeaponElement() { Ref = GetEntity(weapon, TransformUsageFlags.Dynamic) });
            }

            AddComponent(entity, new HealthBarReference()
            {
                Value = GetEntity(authoring.HealthBar, TransformUsageFlags.Dynamic),
            });
        }

        protected override TransformUsageFlags GetUsageFlags() => TransformUsageFlags.Dynamic;
    }
}
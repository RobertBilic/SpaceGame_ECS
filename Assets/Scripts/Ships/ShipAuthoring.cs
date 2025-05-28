using SpaceGame.Combat.Components;
using SpaceGame.Combat.StateTransition.Components;
using SpaceGame.Detection.Component;
using SpaceGame.Movement.Components;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace SpaceGame.Combat.Authoring
{
    public class ShipAuthoring : MonoWithHitbox
    {
        [Header("Sprite Sorting")]
        public int SpriteSortingOrder;
        [Header("Movement")]
        public float Speed;
        public float Acceleration;
        public float Decceleration;
        [Range(0.0f,1.0f)]
        public float SpeedRotationPenalty;
        public float RotationSpeed;

        public float MaxShipBankingAngle;
        public float BankingSmoothSpeed;

        [Header("Separation")]
        public float SeparationRadius;
        public float SeparationStrength;

        [Header("Combat")]
        public float CombatPower;
        public float DetectionRange;
        public List<ForwardWeaponAuthoring> Weapons;
        public List<TurretAuthoring> Turrets;

        [Header("Health")]
        public float Health;
        public GameObject HealthBar;
        [Header("Additional")]
        public List<AdditionalBakedComponentBase> AdditionalComponents;
    }

    class TestEnemyBaker : BakerWithHitboxes<ShipAuthoring>
    {
        protected override void BakeAdditionalData(Entity entity, ShipAuthoring authoring)
        {
            AddComponent(entity, new CombatPower() { Value = authoring.CombatPower });
            AddComponent(entity, new Health() { Current = authoring.Health, Max = authoring.Health });
            AddComponent(entity, new RotationSpeed() { Value = authoring.RotationSpeed });
            AddComponent(entity, new CurrentRotation() { Value = 0.0f });
            AddComponent(entity, new ShipMovementBehaviourState() { Value = ShipMovementBehaviour.MoveToTarget });
            AddComponent(entity, new DetectionRange() { Value = authoring.DetectionRange });
            AddComponent(entity, new SeparationSettings() { RepulsionRadius = authoring.SeparationRadius, RepulsionStrength = authoring.SeparationStrength });
            AddComponent(entity, new MovementDirection());
            AddComponent(entity, new SpriteSortingRoot() { BaseOrder = authoring.SpriteSortingOrder });
            AddComponent<Velocity>(entity);
            AddComponent(entity, new ShipBankingData()
            {
                CurrentBankAngle = 0,
                MaxBankAngle = authoring.MaxShipBankingAngle,
                SmoothSpeed = authoring.BankingSmoothSpeed
            });

            AddComponent(entity, new ThrustSettings() { 
                MaxSpeed = authoring.Speed,
                Acceleration = authoring.Acceleration,
                Decceleration = authoring.Decceleration,
                SpeedRotationPenalty = authoring.SpeedRotationPenalty
            }); 

            AddComponent(entity, new Target());

            var weaponBuffer = AddBuffer<ForwardWeaponElement>(entity);

            if (authoring.Weapons.Count != 0)
            {
                foreach (var weapon in authoring.Weapons)
                    weaponBuffer.Add(new ForwardWeaponElement() { Ref = GetEntity(weapon, TransformUsageFlags.Dynamic) });
            }

            var turretBuffer = AddBuffer<TurretElement>(entity);

            if(authoring.Turrets.Count != 0)
            {
                foreach (var turret in authoring.Turrets)
                    turretBuffer.Add(new TurretElement() { Ref = GetEntity(turret, TransformUsageFlags.Dynamic) });
            }

            AddComponent(entity, new HealthBarReference()
            {
                Value = GetEntity(authoring.HealthBar, TransformUsageFlags.Dynamic),
            });

            foreach (var comp in authoring.AdditionalComponents)
                comp.AddComponent(this, entity);

            AddBuffer<CombatStateChangeWeight>(entity);
            AddBuffer<ExistingCombatStateSpecificComponent>(entity);
            AddBuffer<NewCombatStateSpecificComponent>(entity);
        }

        protected override TransformUsageFlags GetUsageFlags() => TransformUsageFlags.Dynamic;
    }
}
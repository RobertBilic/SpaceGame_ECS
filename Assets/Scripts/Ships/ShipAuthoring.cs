using SpaceGame.Combat.Components;
using SpaceGame.Combat.Defences;
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

        [Header("Defences")]
        public float Shield;
        public float Armor;
        public float Hull;
        public ResistanceMatrixAsset ResistanceAsset;
        public GameObject HealthBar;
        [Header("Destruction VFX")]
        public OnDestructionVFXAuthoring DestructionVFXPrefab;
        [Header("Additional")]
        public List<AdditionalBakedComponentBase> AdditionalComponents;
    }

    class TestEnemyBaker : BakerWithHitboxes<ShipAuthoring>
    {
        protected override void BakeAdditionalData(Entity entity, ShipAuthoring authoring)
        {
            var defenceLayerBuffer = AddBuffer<DefenceLayer>(entity);

            defenceLayerBuffer.Add(new DefenceLayer()
            {
                Max = authoring.Shield,
                Type = DefenceLayerType.Shield,
                Value = authoring.Shield
            });
            defenceLayerBuffer.Add(new DefenceLayer()
            {
                Max = authoring.Armor,
                Type = DefenceLayerType.Armor,
                Value = authoring.Armor
            });
            defenceLayerBuffer.Add(new DefenceLayer()
            {
                Max = authoring.Hull,
                Type = DefenceLayerType.Hull,
                Value = authoring.Hull
            });

            var resistanceBuffer = AddBuffer<ResistanceEntry>(entity);

            if(authoring.ResistanceAsset != null)
            {
                foreach(var entry in authoring.ResistanceAsset.Entries)
                {
                    resistanceBuffer.Add(new ResistanceEntry()
                    {
                        Layer = entry.LayerType,
                        Resistance = entry.Value,
                        Type = entry.DamageType
                    });
                }
            }

            AddComponent(entity, new CombatPower() { Value = authoring.CombatPower });
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

            if (authoring.DestructionVFXPrefab != null)
                AddComponent(entity, new OnDestructionVFXPrefab() { Prefab = GetEntity(authoring.DestructionVFXPrefab, TransformUsageFlags.Dynamic) });
        }

        protected override TransformUsageFlags GetUsageFlags() => TransformUsageFlags.Dynamic;
    }
}
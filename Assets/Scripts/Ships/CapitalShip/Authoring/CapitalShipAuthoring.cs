using SpaceGame.Movement.Components;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace SpaceGame.Combat.Authoring
{
    public class CapitalShipAuthoring : MonoWithHitbox
    {
        public float MoveSpeed;
        public float RotationSpeed;

        [Header("Health")]
        public float MaxHealth;
        public GameObject HealthBar;
    }

    class CapitalShipBaker : BakerWithHitboxes<CapitalShipAuthoring>
    {
        protected override void BakeAdditionalData(Entity entity, CapitalShipAuthoring authoring)
        {
            AddComponent(entity, new MoveSpeed { Value = authoring.MoveSpeed });
            AddComponent(entity, new RotationSpeed { Value = authoring.RotationSpeed });
            AddComponent(entity, new CapitalShipTag());

            if (authoring.HealthBar != null)
            {
                AddComponent(entity, new HealthBarReference()
                {
                    Value = GetEntity(authoring.HealthBar, TransformUsageFlags.Dynamic)
                });
            }
        }

        protected override TransformUsageFlags GetUsageFlags() => TransformUsageFlags.Dynamic;
    }
}
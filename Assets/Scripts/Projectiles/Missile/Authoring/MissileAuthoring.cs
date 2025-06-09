using SpaceGame.Combat.Components;
using SpaceGame.Movement.Components;
using Unity.Entities;
using UnityEngine;

class MissileAuthoring : MonoWithHitbox
{
    public float Scale;
    public float Speed;
    public float RotationSpeed;
    public float ExplosionRadius;
    [Header("On Hit Effect")]
    public GameObject OnHitPrefab;
    public float OnHitLifetime;

    class MissileAuthoringBaker : BakerWithHitboxes<MissileAuthoring>
    {

        protected override void BakeAdditionalData(Entity entity, MissileAuthoring authoring)
        {
            AddComponent(entity, new MissileTag());
            AddComponent(entity, new ThrustSettings() { MaxSpeed = authoring.Speed });
            AddComponent(entity, new MissileSettings() { MoveSpeed = authoring.Speed, TurnSpeed = authoring.RotationSpeed });
            AddComponent(entity, new ProjectileTag());
            AddComponent(entity, new ProjectileScale() { Value = authoring.Scale });
            AddComponent(entity, new Lifetime { Value = 0.0f });
            AddComponent(entity, new Heading() { });
            AddComponent(entity, new PreviousPosition() { });
            AddComponent(entity, new Damage() { });
            AddComponent(entity, new TeamTag());
            AddComponent(entity, new ProjectileId() { });
            AddComponent(entity, new Target());
            AddComponent(entity, new Disabled());
            AddComponent(entity, new NeedsBoundingRadiusScalingTag());
            AddComponent(entity, new ExplosionRadius() { Value = authoring.ExplosionRadius });

            if (authoring.OnHitPrefab != null)
            {
                AddComponent(entity, new OnHitEffectPrefab()
                {
                    Value = GetEntity(authoring.OnHitPrefab, TransformUsageFlags.Dynamic),
                    Lifetime = authoring.OnHitLifetime
                });
            }
        }

        protected override TransformUsageFlags GetUsageFlags() => TransformUsageFlags.Dynamic;
    }

}

using SpaceGame.Combat.Components;
using SpaceGame.Movement.Components;
using Unity.Entities;
using UnityEngine;

namespace SpaceGame.Combat.Authoring
{
    public class BulletAuthoring : MonoBehaviour
    {
        public float Speed;
        public float Radius;
        
        [Header("On Hit Effect")]
        public GameObject OnHitPrefab;
        public float OnHitLifetime;
    }

    class BulletAuthoringBaker : Baker<BulletAuthoring>
    {
        public override void Bake(BulletAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new ThrustSettings() { MaxSpeed = authoring.Speed });
            AddComponent(entity, new Radius() { Value = authoring.Radius });
            AddComponent(entity, new ProjectileScale() { Value = authoring.Radius * 2.0f });
            AddComponent(entity, new BulletTag());
            AddComponent(entity, new ProjectileTag());
            AddComponent(entity, new Lifetime { Value = 0.0f });
            AddComponent(entity, new Heading() { });
            AddComponent(entity, new PreviousPosition() { });
            AddComponent(entity, new Damage() { });
            AddComponent(entity, new TeamTag());
            AddComponent(entity, new Target());
            AddComponent(entity, new OnHitEffectPrefab()
            {
                Value = GetEntity(authoring.OnHitPrefab, TransformUsageFlags.Dynamic),
                Lifetime = authoring.OnHitLifetime
            });
            AddComponent(entity, new ProjectileId() { });
            AddComponent(entity, new Disabled());
        }
    }
}
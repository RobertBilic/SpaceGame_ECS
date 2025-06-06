using SpaceGame.Combat.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace SpaceGame.Combat.Authoring
{
    class OnHitEffectAuthoring : MonoBehaviour
    {
        public Color Color;
    }

    class OnHitEffectAuthoringBaker : Baker<OnHitEffectAuthoring>
    {
        public override void Bake(OnHitEffectAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Renderable);
            var c = authoring.Color;

            AddComponent(entity, new MaterialProperty__Fade() { Value = 1.0f });
            AddComponent(entity, new MaterialProperty__Color() { Value = new float4(c.r, c.g, c.b, c.a) });
            AddComponent(entity, new ImpactParticle());
            AddComponent(entity, new ProjectileId());
            AddComponent(entity, new Disabled());
        }
    }
}
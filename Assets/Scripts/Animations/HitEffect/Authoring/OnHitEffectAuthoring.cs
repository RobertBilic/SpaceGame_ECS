using SpaceGame.Combat.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace SpaceGame.Combat.Authoring
{
    class OnHitEffectAuthoring : MonoBehaviour
    {
    }

    class OnHitEffectAuthoringBaker : Baker<OnHitEffectAuthoring>
    {
        public override void Bake(OnHitEffectAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Renderable);

            AddComponent(entity, new ImpactParticle());
            AddComponent(entity, new ProjectileId());
            AddComponent(entity, new Disabled());
        }
    }
}
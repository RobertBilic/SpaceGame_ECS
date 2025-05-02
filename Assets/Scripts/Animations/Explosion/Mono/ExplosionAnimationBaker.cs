using SpaceGame.Animations.Components;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace SpaceGame.Animations.Authoring
{
    class ExplosionAnimationBaker : MonoBehaviour
    {
        public List<ExplosionSpriteElementHolder> Elements;
        public uint AnimationSeed;
    }

    class ExplosionAnimationBakerBaker : Baker<ExplosionAnimationBaker>
    {
        public override void Bake(ExplosionAnimationBaker authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new RandomGenerator() { Value = Unity.Mathematics.Random.CreateFromIndex(authoring.AnimationSeed) });
            var buffer = AddBuffer<ExplosionSpriteElement>(entity);

            foreach (var holder in authoring.Elements)
            {
                buffer.Add(new ExplosionSpriteElement()
                {
                    SpriteEntity = GetEntity(holder.Element, TransformUsageFlags.Dynamic),
                    TimeOnElementMax = holder.MaxTime,
                    TimeOnElementMin = holder.MinTime
                });
            }
        }
    }

    [System.Serializable]
    class ExplosionSpriteElementHolder
    {
        public GameObject Element;
        public float MinTime;
        public float MaxTime;
    }
}
using SpaceGame.Animations.Components;
using Unity.Entities;
using UnityEngine;

namespace SpaceGame.Animations.Authoring
{
    class ExplosionAnimationPrefabAuthoring : MonoBehaviour
    {
        public GameObject Prefab;
    }

    class ExplosionAnimationPrefabAuthoringBaker : Baker<ExplosionAnimationPrefabAuthoring>
    {
        public override void Bake(ExplosionAnimationPrefabAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new ExplosionPrefab() { Value = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic) });
        }
    }
}
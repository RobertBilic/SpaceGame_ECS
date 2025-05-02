using SpaceGame.Combat.Components;
using Unity.Entities;
using UnityEngine;

namespace SpaceGame.Combat.Authoring
{
    class BulletPrefabAuthoring : MonoBehaviour
    {
        public GameObject Prefab;
        public float BulletSpeed;
        public float Scale;
    }

    class BulletPrefabAuthoringBaker : Baker<BulletPrefabAuthoring>
    {
        public override void Bake(BulletPrefabAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            var prefabEntity = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic);


            AddComponent<Prefab>(entity);
            AddComponent(entity, new BulletPrefabData()
            {
                Entity = prefabEntity,
                BulletSpeed = authoring.BulletSpeed,
                Scale = authoring.Scale
            });
        }
    }
}
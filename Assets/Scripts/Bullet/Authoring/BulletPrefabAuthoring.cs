using SpaceGame.Combat.Components;
using Unity.Entities;
using UnityEngine;

namespace SpaceGame.Combat.Authoring
{
    class BulletPrefabAuthoring : MonoBehaviour
    {
        public string Id;
        public GameObject Prefab;

        private void OnValidate()
        {
            if(Id.Length > 29)
            {
                UnityEngine.Debug.LogError("Strings must have less than 30 characters since we use FixedString32Bytes");
            }
        }
    }

    class BulletPrefabAuthoringBaker : Baker<BulletPrefabAuthoring>
    {
        public override void Bake(BulletPrefabAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            var prefabEntity = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic);

            AddComponent<Prefab>(entity);
            AddComponent(entity, new BulletPrefab()
            {
                Entity = prefabEntity,
                Id = authoring.Id
            }); 
        }
    }
}
using SpaceGame.Combat.Components;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace SpaceGame.Combat.Authoring
{
    class BulletPrefabAuthoring : MonoBehaviour
    {
        [SerializeField]
        private BulletPrefabHolder prefabHolder;

        private void OnValidate()
        {
            foreach (var data in prefabHolder.Data)
            {
                if (data.Id.Length > 29)
                {
                    UnityEngine.Debug.LogError("Strings must have less than 30 characters since we use FixedString32Bytes");
                }
            }
        }

        class BulletPrefabAuthoringBaker : Baker<BulletPrefabAuthoring>
        {
            public override void Bake(BulletPrefabAuthoring authoring)
            {

                foreach (var data in authoring.prefabHolder.Data)
                {
                    var entity = CreateAdditionalEntity(TransformUsageFlags.None);
                    var prefabEntity = GetEntity(data.Prefab.gameObject, TransformUsageFlags.Dynamic);

                    AddComponent<Prefab>(entity);
                    AddComponent(entity, new BulletPrefab()
                    {
                        Entity = prefabEntity,
                        Id = data.Id
                    });
                }
            }
        }
    }
}
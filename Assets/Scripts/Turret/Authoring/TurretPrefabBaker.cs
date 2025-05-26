using SpaceGame.Combat.Components;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace SpaceGame.Combat.Authoring
{
    class TurretPrefabBaker : MonoBehaviour
    {
        [SerializeField]
        private TurretPrefabDataHolder dataHolder;

        class TurretPrefabBakerBaker : Baker<TurretPrefabBaker>
        {

            public override void Bake(TurretPrefabBaker authoring)
            {
                foreach (var data in authoring.dataHolder.Data)
                {
                    var rootEntity = CreateAdditionalEntity(TransformUsageFlags.None);
                    var prefabEntity = GetEntity(data.Prefab.gameObject, TransformUsageFlags.Dynamic);

                    AddComponent(rootEntity, new TurretPrefab()
                    {
                        PrefabEntity = prefabEntity,
                        Id = data.Id
                        
                    });

                    AddComponent<Prefab>(rootEntity);
                }
            }
        }
    }
}
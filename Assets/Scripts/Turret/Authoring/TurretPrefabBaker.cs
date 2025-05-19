using SpaceGame.Combat.Components;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace SpaceGame.Combat.Authoring
{
    class TurretPrefabBaker : MonoBehaviour
    {
        [SerializeField]
        private List<TurretPrefabData> dataList;

        class TurretPrefabBakerBaker : Baker<TurretPrefabBaker>
        {

            public override void Bake(TurretPrefabBaker authoring)
            {
                foreach (var data in authoring.dataList)
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
        
        [System.Serializable]
        class TurretPrefabData
        {
            public TurretPropertyHolder Prefab;
            public string Id;
        }
    }
}
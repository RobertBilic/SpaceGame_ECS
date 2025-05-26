using SpaceGame.Combat.Components;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace SpaceGame.Combat.Authoring
{
    public class ShipPrefabAuthoring : MonoBehaviour
    {
        [SerializeField]
        private ShipPrefabDataHolder PrefabHolder;

        public class CapitalShipPrefabBaker : Baker<ShipPrefabAuthoring>
        {
            public override void Bake(ShipPrefabAuthoring authoring)
            {
                foreach (var data in authoring.PrefabHolder.Data)
                {
                    var prefabEntity = CreateAdditionalEntity(TransformUsageFlags.None, false, "ShipPrefab:" + data.Id);
                    var prefab = GetEntity(data.Prefab.gameObject, TransformUsageFlags.Dynamic);

                    AddComponent<Prefab>(prefabEntity);
                    AddComponent(prefabEntity, new ShipPrefab() { Value = prefab , Id = data.Id, Scale = data.DefaultScale });
                }
            }
        }
    }
}
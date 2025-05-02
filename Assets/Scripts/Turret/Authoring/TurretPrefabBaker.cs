using Unity.Entities;
using UnityEngine;

class TurretPrefabBaker : MonoBehaviour
{
    public TurretPropertyHolder Prefab;
}

class TurretPrefabBakerBaker : Baker<TurretPrefabBaker>
{
    public override void Bake(TurretPrefabBaker authoring)
    {
        var rootEntity = GetEntity(TransformUsageFlags.None);
        var prefabEntity = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic);

        AddComponent(rootEntity, new TurretPrefab() { 
            PrefabEntity = prefabEntity
        });

        AddComponent<Prefab>(rootEntity);
    }
}


public struct TurretPrefab : IComponentData
{
    public Entity PrefabEntity;
}
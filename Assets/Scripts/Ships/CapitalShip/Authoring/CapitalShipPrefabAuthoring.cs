using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Transforms;

public class CapitalShipPrefabAuthoring : MonoBehaviour
{
    public GameObject Prefab;
}

public class CapitalShipPrefabBaker : Baker<CapitalShipPrefabAuthoring>
{
    public override void Bake(CapitalShipPrefabAuthoring authoring)
    {
        var rootEntity = GetEntity(TransformUsageFlags.None);
        var prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic);

        AddComponent<Prefab>(rootEntity);
        AddComponent(rootEntity, new CapitalShipPrefab() { Value = prefab });

    }
}


public struct CapitalShipPrefab :IComponentData
{
    public Entity Value;
}

using Unity.Entities;
using UnityEngine;

class HealthBarProgressAuthoring : MonoBehaviour
{
}

class HealthBarProgressAuthoringBaker : Baker<HealthBarProgressAuthoring>
{
    public override void Bake(HealthBarProgressAuthoring authoring)
    {
        var entity = GetEntity(authoring.gameObject,TransformUsageFlags.Dynamic);
    }
}

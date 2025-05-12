using Unity.Entities;
using UnityEngine;

class HealthBarProgressAuthoring : MonoBehaviour
{
    public float EdgeFade = 0.05f;
    public Color Color;
}

class HealthBarProgressAuthoringBaker : Baker<HealthBarProgressAuthoring>
{
    public override void Bake(HealthBarProgressAuthoring authoring)
    {
        var entity = GetEntity(authoring.gameObject,TransformUsageFlags.Dynamic);
        var c = authoring.Color;

        AddComponent(entity, new FillMaterialOverrideComponent() { Value = 1.0f });
        AddComponent(entity, new EdgeFadeMaterialOverride() { Value = authoring.EdgeFade });
        AddComponent(entity, new ColorMaterialOverride() { Value =  new Unity.Mathematics.float4(c.r,c.g,c.b,c.a)});
    }
}

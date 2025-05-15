using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

class HealthBarAuthoring : MonoBehaviour
{
    public GameObject Parent;
    public float EdgeFade = 0.05f;
    public Color Color;

    class HealthBarAuthoringBaker : Baker<HealthBarAuthoring>
    {
        public override void Bake(HealthBarAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            var c = authoring.Color;

            AddComponent(entity, new LocalOffsetCorrection() { Value = authoring.transform.localPosition });
            AddComponent(entity, new FillMaterialOverrideComponent() { Value = 1.0f });
            AddComponent(entity, new EdgeFadeMaterialOverride() { Value = authoring.EdgeFade });
            AddComponent(entity, new ColorMaterialOverride() { Value = new Unity.Mathematics.float4(c.r, c.g, c.b, c.a) });

            if (authoring.Parent != null)
                AddComponent(entity, new Parent() { Value = GetEntity(authoring.Parent, TransformUsageFlags.Dynamic) });
        }
    }
}

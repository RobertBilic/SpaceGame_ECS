using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

class HealthBarAuthoring : MonoBehaviour
{
    public GameObject Parent;
    public float EdgeFade = 0.05f;

    class HealthBarAuthoringBaker : Baker<HealthBarAuthoring>
    {
        public override void Bake(HealthBarAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            var scale = authoring.transform.localScale;

            AddComponent(entity, new LocalOffsetCorrection() { Value = authoring.transform.localPosition });
            AddComponent(entity, new FillMaterialOverrideComponent() { Value = 1.0f });
            AddComponent(entity, new EdgeFadeMaterialOverride() { Value = authoring.EdgeFade });
            AddComponent(entity, new NeedsHealthBarRecoloring());
            AddComponent(entity, new ColorMaterialOverride() { Value = new float4(0.0f, 0.0f, 0.0f, 0.0f) });
            AddComponent(entity, new PostTransformMatrix() { Value = float4x4.Scale(scale.x, scale.y, scale.z) });
            AddComponent(entity, new LocalToWorld());

            if(authoring.gameObject.activeSelf)
                AddComponent(entity, new Disabled());

            if (authoring.Parent != null)
                AddComponent(entity, new Parent() { Value = GetEntity(authoring.Parent, TransformUsageFlags.Dynamic) });
        }
    }
}

using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

class HealthBarAuthoring : MonoBehaviour
{
    public GameObject Parent;

    class HealthBarAuthoringBaker : Baker<HealthBarAuthoring>
    {
        public override void Bake(HealthBarAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new LocalOffsetCorrection() { Value = authoring.transform.localPosition });
            if (authoring.Parent != null)
                AddComponent(entity, new Parent() { Value = GetEntity(authoring.Parent, TransformUsageFlags.Dynamic) });
        }
    }
}

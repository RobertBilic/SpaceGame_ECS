using Unity.Entities;
using UnityEngine;

class TrailRendererAuthoring : MonoBehaviour
{
    public string Id;

    class TrailRendererAuthoringBaker : Baker<TrailRendererAuthoring>
    {
        public override void Bake(TrailRendererAuthoring authoring)
        {
            var entity = GetEntity(authoring.gameObject,TransformUsageFlags.Dynamic);
            AddComponent(entity, new TrailRendererTag()
            {
                Id = authoring.Id,
            });
            AddBuffer<SpatialDatabaseCellIndex>(entity);
        }
    }
}


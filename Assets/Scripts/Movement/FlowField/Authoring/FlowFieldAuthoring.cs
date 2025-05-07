using SpaceGame.Movement.Flowfield.Components;
using Unity.Entities;
using UnityEngine;

namespace SpaceGame.Movement.Flowfield.Authoring
{
    public class FlowFieldAuthoring : MonoBehaviour
    {
        [SerializeField]
        private Vector2 WorldSize;
        [SerializeField]
        private float CellSize;

        public class Baker : Baker<FlowFieldAuthoring>
        {
            public override void Bake(FlowFieldAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new FlowFieldSettings
                {
                    WorldSize = authoring.WorldSize,
                    CellSize = authoring.CellSize
                });

                AddBuffer<FlowFieldCell>(entity);
            }
        }
    }
}
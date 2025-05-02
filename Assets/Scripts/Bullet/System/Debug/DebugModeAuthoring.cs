using SpaceGame.Debug.Components;
using Unity.Entities;
using UnityEngine;

namespace SpaceGame.Debug.Authoring
{
    class DebugModeAuthoring : MonoBehaviour
    {
        public bool UseDebugMode;
    }

    class DebugModeAuthoringBaker : Baker<DebugModeAuthoring>
    {
        public override void Bake(DebugModeAuthoring authoring)
        {
            if (authoring.UseDebugMode)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                AddComponent<EnableDebugTag>(entity);
            }
        }
    }
}
using SpaceGame.Debug.Components;
using Unity.Entities;
using UnityEngine;

namespace SpaceGame.Debug.Authoring
{
    class DebugModeAuthoring : MonoBehaviour
    {
        public bool DebugCollisions;
        public bool DebugForwardWeapons;
    }

    class DebugModeAuthoringBaker : Baker<DebugModeAuthoring>
    {
        public override void Bake(DebugModeAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            if (authoring.DebugCollisions)
                AddComponent<EnableCollisionDebuggingTag>(entity);

            if (authoring.DebugForwardWeapons)
                AddComponent<EnableForwardWeaponDebug>(entity);
        }
    }
}
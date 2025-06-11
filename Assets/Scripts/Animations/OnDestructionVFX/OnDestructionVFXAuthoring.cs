using SpaceGame.Combat.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace SpaceGame.Combat.Authoring
{
    public class OnDestructionVFXAuthoring : MonoBehaviour
    {
        [SerializeField]
        private float lifetime;

        class OnDestructionVFXAuthoringBaker : Baker<OnDestructionVFXAuthoring>
        {
            public override void Bake(OnDestructionVFXAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Renderable);

                AddComponent(entity, new OnDestructionVFXTag());
                AddComponent(entity, new Lifetime() { Value = authoring.lifetime});
            }
        }
    }

}
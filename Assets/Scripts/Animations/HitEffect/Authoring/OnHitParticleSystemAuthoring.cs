using Unity.Entities;
using UnityEngine;

class OnHitParticleSystemAuthoring : MonoBehaviour
{
    class OnHitParticleSystemAuthoringBaker : Baker<OnHitParticleSystemAuthoring>
    {
        public override void Bake(OnHitParticleSystemAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new AutoplayParticleSystem());
        }
    }

}
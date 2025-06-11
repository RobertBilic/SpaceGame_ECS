using Unity.Entities;
using UnityEngine;

class AutoplayPSAuthoring : MonoBehaviour
{
    class AutoplayPSAuthoringBaker : Baker<AutoplayPSAuthoring>
    {
        public override void Bake(AutoplayPSAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new AutoplayParticleSystem());
        }
    }

}
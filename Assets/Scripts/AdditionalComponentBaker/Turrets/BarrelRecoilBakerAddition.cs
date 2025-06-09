using SpaceGame.Animations.Components;
using UnityEngine;

namespace SpaceGame.Combat.Authoring
{
    class BarrelRecoilBakerAddition : AdditionalBakedComponent<BarrelRecoilReference>
    {
        public GameObject RecoilTarget;
        public float RecoilDuration;
        public float MaxRecoilDistance;

        protected override BarrelRecoilReference GetComponentData<TAuthoring>(Unity.Entities.Baker<TAuthoring> baker)
        {
            return new BarrelRecoilReference()
            {
                Entity = baker.GetEntity(RecoilTarget, Unity.Entities.TransformUsageFlags.Dynamic),
                DefaultPosition = RecoilTarget.transform.position,
                Duration = RecoilDuration,
                MaxDistance = MaxRecoilDistance
            };
        }
    }
}

using SpaceGame.Animations.Components;
using SpaceGame.Combat.Components;
using SpaceGame.Movement.Components;
using Unity.Entities;

namespace SpaceGame.Combat.Authoring
{
    class TurretPropertyHolderBakerBaker : Baker<TurretPropertyHolder>
    {
        public override void Bake(TurretPropertyHolder authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new IsAlive() { });
            AddComponent(entity, new RotationSpeed() { Value = authoring.RotationSpeed });
            AddComponent(entity, new Range() { Value = authoring.Range });
            AddComponent(entity, new Damage() { Value = authoring.Damage });
            AddComponent(entity, new FiringRate() { Value = authoring.FiringRate });
            AddComponent(entity, new LastFireTime() { Value = 0.0f });
            AddComponent(entity, new TurretTag());
            AddComponent(entity, new RotationBaseReference() { RotationBase = GetEntity(authoring.RotationBase, TransformUsageFlags.Dynamic) });
            AddComponent(entity, new Heading() { Value = new Unity.Mathematics.float3(1, 0, 0) });


            AddComponent(entity, new BulletId() { Value = authoring.BulletId });
            if (authoring.RecoilTarget != null)
            {
                AddComponent(entity, new BarrelRecoilReference()
                {
                    Duration = authoring.RecoilDuration,
                    MaxDistance = authoring.MaxRecoilDistance,
                    Entity = GetEntity(authoring.RecoilTarget, TransformUsageFlags.Dynamic),
                    DefaultPosition = authoring.RecoilTarget.transform.localPosition
                });
            }

            var spawnOffsetBuffer = AddBuffer<ProjectileSpawnOffset>(entity);

            foreach (var offset in authoring.bulletSpawnPositionsLocal)
            {
                spawnOffsetBuffer.Add(new ProjectileSpawnOffset() { Value = offset });
            }
        }
    }
}
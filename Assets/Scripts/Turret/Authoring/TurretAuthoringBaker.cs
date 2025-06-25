using SpaceGame.Animations.Components;
using SpaceGame.Combat.Components;
using SpaceGame.Movement.Components;
using Unity.Entities;

namespace SpaceGame.Combat.Authoring
{
    class TurretAuthoringBaker : Baker<TurretAuthoring>
    {
        public override void Bake(TurretAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);


            AddComponent(entity, new ProjectileReloadData()
            {
                CurrentReloadTime = 0.0f,
                ReloadTime = authoring.ReloadTime
            });
            AddComponent(entity, new ProjectileClipSize()
            {
                MaxSize = authoring.AmmoSize,
                CurrentSize = authoring.AmmoSize
            });

            AddComponent(entity, new IsAlive() { });
            AddComponent(entity, new RotationSpeed() { Value = authoring.RotationSpeed });
            AddComponent(entity, new Range() { Value = authoring.Range });
            AddComponent(entity, new Damage() { Value = authoring.Damage, Type = authoring.DamageType });
            AddComponent(entity, new FiringRate() { Value = authoring.FiringRate });
            AddComponent(entity, new LastFireTime() { Value = 0.0f });
            AddComponent(entity, new TurretTag());
            AddComponent(entity, new RotationBaseReference() { RotationBase = GetEntity(authoring.RotationBase, TransformUsageFlags.Dynamic) });
            AddComponent(entity, new Heading() { Value = new Unity.Mathematics.float3(1, 0, 0) });
            AddComponent(entity, new ProjectileId() { Value = authoring.BulletId });

            var spawnOffsetBuffer = AddBuffer<ProjectileSpawnOffset>(entity);

            foreach (var offset in authoring.bulletSpawnPositionsLocal)
            {
                spawnOffsetBuffer.Add(new ProjectileSpawnOffset() { Value = offset });
            }

            foreach(var comp in authoring.AdditionalComponents)
            {
                comp.AddComponent<TurretAuthoring>(this, entity);
            }

            foreach(var additionalBuffer in authoring.AdditionalBuffers)
            {
                additionalBuffer.AddBuffer(this, entity);
            }
        }
    }
}
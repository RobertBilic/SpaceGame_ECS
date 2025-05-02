using Unity.Entities;
using UnityEngine;


class TurretPropertyHolderBakerBaker : Baker<TurretPropertyHolder>
{
    public override void Bake(TurretPropertyHolder authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);

        AddComponent(entity, new IsAlive() { Value = true });
        AddComponent(entity, new RotationSpeed() { Value = authoring.RotationSpeed });
        AddComponent(entity, new Range() { Value = authoring.Range });
        AddComponent(entity, new Damage() { Value = authoring.Damage });
        AddComponent(entity, new FiringRate() { Value = authoring.FiringRate });
        AddComponent(entity, new LastFireTime() { Value = 0.0f });
        AddComponent(entity, new Cooldown() { Value = 0 });
        AddComponent(entity, new TurretTag());
        AddComponent(entity, new TurretRotationBaseReference() { RotationBase = GetEntity(authoring.RotationBase, TransformUsageFlags.Dynamic) });
        AddComponent(entity, new Heading() { Value = new Unity.Mathematics.float3(1, 0, 0)});
        
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

        var spawnOffsetBuffer = AddBuffer<TurretProjectileSpawnOffset>(entity);

        foreach(var offset in authoring.bulletSpawnPositionsLocal)
        {
            spawnOffsetBuffer.Add(new TurretProjectileSpawnOffset() { Value = offset });
        }
    }
}

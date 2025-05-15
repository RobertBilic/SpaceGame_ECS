using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(CombatMovementGroup))]
public partial class ImpactParticleSystem : SystemBase
{
    protected override void OnUpdate()
    {
        if (!SystemAPI.TryGetSingleton<GlobalTimeComponent>(out var timeComp))
            return;

        float dt = timeComp.DeltaTime;

        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        foreach (var (particle, transform, bulletId,entity) in
                 SystemAPI.Query<RefRW<ImpactParticle>, RefRW<LocalTransform>,RefRO<BulletId>>().WithEntityAccess())
        {
            particle.ValueRW.Age += dt;
            if (particle.ValueRW.Age >= particle.ValueRW.Lifetime)
            {
                if (SystemAPI.TryGetSingletonBuffer<ImpactEffectPoolRequest>(out var impactPoolCollector))
                {
                    impactPoolCollector.Add(new ImpactEffectPoolRequest()
                    {
                        Entity = entity,
                        Id = bulletId.ValueRO.Value
                    });
                }
                continue;
            }

            transform.ValueRW.Position += particle.ValueRW.Velocity * dt;
            ecb.SetComponent(entity, new MaterialProperty__Fade() { Value = 1.0f - (particle.ValueRO.Age / particle.ValueRO.Lifetime) });
        }

        if(ecb.ShouldPlayback)
        {
            ecb.Playback(EntityManager);
        }
        ecb.Dispose();
    }
}

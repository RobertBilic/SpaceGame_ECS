using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class ImpactParticleSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float dt = SystemAPI.Time.DeltaTime;
        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        foreach (var (particle, transform, entity) in
                 SystemAPI.Query<RefRW<ImpactParticle>, RefRW<LocalTransform>>().WithEntityAccess())
        {
            particle.ValueRW.Age += dt;
            if (particle.ValueRW.Age >= particle.ValueRW.Lifetime)
            {
                ecb.DestroyEntity(entity);
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

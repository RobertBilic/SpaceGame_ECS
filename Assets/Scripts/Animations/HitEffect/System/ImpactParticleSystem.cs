using SpaceGame.Combat.Components;
using Unity.Entities;
using Unity.Transforms;

namespace SpaceGame.Combat.Systems
{
    [UpdateInGroup(typeof(CombatMovementGroup))]
    public partial class ImpactParticleSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            if (!SystemAPI.TryGetSingleton<GlobalTimeComponent>(out var timeComp))
                return;

            float dt = timeComp.DeltaTime;

            EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            foreach (var (particle, bulletId, entity) in
                     SystemAPI.Query<RefRW<ImpactParticle>, RefRO<ProjectileId>>().WithEntityAccess())
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
            }

            if (ecb.ShouldPlayback)
            {
                ecb.Playback(EntityManager);
            }
            ecb.Dispose();
        }
    }
}
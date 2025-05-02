using SpaceGame.Animations.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
[UpdateAfter(typeof(CombatSystemGroup))]
public partial struct DestructionDetectionSystem : ISystem
{
    private bool isInitialized;
    private Entity explosionPrefab;

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

        if (!isInitialized)
        {
            foreach (var prefab in SystemAPI.Query<RefRO<ExplosionPrefab>>().WithOptions(EntityQueryOptions.IncludePrefab))
            {
                isInitialized = true;
                explosionPrefab = prefab.ValueRO.Value;
                break;
            }
        }

        if (!isInitialized)
            return;

        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach (var (localToWorld,health, entity) in SystemAPI.Query<RefRO<LocalToWorld>,RefRO<Health>>().WithNone<PendingDestructionTag>().WithEntityAccess())
        {
            if (health.ValueRO.Value <= 0f)
            {
                ecb.DestroyEntity(entity);

                var explosionEntity = ecb.Instantiate(explosionPrefab);

                ecb.AddComponent(explosionEntity, new LocalTransform()
                {
                    Position = localToWorld.ValueRO.Position,
                    Rotation = Unity.Mathematics.quaternion.identity,
                    Scale = 1.0f
                });

                ecb.AddComponent(explosionEntity, new ExplosionAnimationState()
                {
                    CurrentFrame = 0,
                    TimeSinceLastFrame = 0,
                    TimeUntilNextFrame = 0
                });


                //TODO: For now just destroy it and spawn an explosion
                //ecb.AddComponent<PendingDestructionTag>(entity);
                //ecb.SetComponent(entity, new IsAlive() { Value = false });
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}

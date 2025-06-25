using SpaceGame.Animations.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(CombatLateUpdateGroup), OrderLast = true)]
public partial struct DestructionDetectionSystem : ISystem
{
    private ComponentLookup<OnDestructionVFXPrefab> destructionVFXPrefabLookup;
    private ComponentLookup<BoundingRadius> boundingRadiusLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PendingDestructionTag>();

        destructionVFXPrefabLookup = state.GetComponentLookup<OnDestructionVFXPrefab>(true);
        boundingRadiusLookup = state.GetComponentLookup<BoundingRadius>(true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        destructionVFXPrefabLookup.Update(ref state);
        boundingRadiusLookup.Update(ref state);

        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach (var (localToWorld, entity) in SystemAPI.Query<RefRO<LocalToWorld>>()
            .WithAll<PendingDestructionTag>()
            .WithEntityAccess())
        {
            ecb.DestroyEntity(entity);

            if (destructionVFXPrefabLookup.HasComponent(entity))
            {
                var vfx = ecb.Instantiate(destructionVFXPrefabLookup[entity].Prefab);
                var scale = boundingRadiusLookup.HasComponent(entity) ? boundingRadiusLookup[entity].Value : 1.0f;

                ecb.SetComponent<LocalTransform>(vfx, new LocalTransform()
                {
                    Position = localToWorld.ValueRO.Position,
                    Rotation = localToWorld.ValueRO.Rotation,
                    Scale = scale
                });
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}

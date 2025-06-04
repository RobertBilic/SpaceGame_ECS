using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(CombatInitializationGroup))]
partial struct FleetEntityTransformSystem : ISystem
{
    private ComponentLookup<LocalToWorld> ltwLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        ltwLookup = state.GetComponentLookup<LocalToWorld>(true);    
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        ltwLookup.Update(ref state);

        foreach (var (lt, fleetEntity) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<FleetEntity>>())
        {
            if (fleetEntity.ValueRO.Leader == Entity.Null || !state.EntityManager.Exists(fleetEntity.ValueRO.Leader))
                continue;

            if (!ltwLookup.HasComponent(fleetEntity.ValueRO.Leader))
                continue;

            var leaderLtw = ltwLookup[fleetEntity.ValueRO.Leader];

            lt.ValueRW.Position = leaderLtw.Position;
            lt.ValueRW.Rotation = leaderLtw.Rotation;
            lt.ValueRW.Scale = 1.0f;
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}

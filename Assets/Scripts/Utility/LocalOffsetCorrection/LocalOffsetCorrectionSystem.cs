using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct LocalOffsetCorrectionSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<LocalOffsetCorrection>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach(var (lt, ltw, offset, parent, entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRW<LocalToWorld>, RefRO<LocalOffsetCorrection>, RefRO<Parent>>()
            .WithOptions(EntityQueryOptions.IncludeDisabledEntities)
            .WithEntityAccess())
        {
            if (!SystemAPI.HasComponent<LocalToWorld>(parent.ValueRO.Value))
                continue;

            var parentLtw = SystemAPI.GetComponent<LocalToWorld>(parent.ValueRO.Value);

            float3 worldOffset = offset.ValueRO.Value;
            var inverseRot = math.inverse(parentLtw.Rotation);
            float3 correctedLocalPos = math.mul(inverseRot, worldOffset);
            lt.ValueRW.Position = correctedLocalPos;
            lt.ValueRW.Rotation = inverseRot;

            if (state.EntityManager.HasComponent<Disabled>(entity))
                ltw.ValueRW.Value = math.mul(parentLtw.Value, lt.ValueRO.ToMatrix());
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}

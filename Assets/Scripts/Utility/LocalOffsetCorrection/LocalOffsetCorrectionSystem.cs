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
        foreach(var (lt, offset,parent) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<LocalOffsetCorrection>, RefRO<Parent>>()
            .WithOptions(EntityQueryOptions.IncludeDisabledEntities))
        {
            if (!SystemAPI.HasComponent<LocalToWorld>(parent.ValueRO.Value))
                continue;

            var ltw = SystemAPI.GetComponent<LocalToWorld>(parent.ValueRO.Value);

            float3 worldOffset = offset.ValueRO.Value;
            var inverseRot = math.inverse(ltw.Rotation);
            float3 correctedLocalPos = math.mul(inverseRot, worldOffset);
            lt.ValueRW.Position = correctedLocalPos;
            lt.ValueRW.Rotation = inverseRot;
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}

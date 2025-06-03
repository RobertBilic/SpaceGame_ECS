using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(CombatInitializationGroup))]
public partial struct FleetCohesionCalculationSystem : ISystem
{
    private NativeParallelHashMap<Entity, float> fleetCohesionMap;
    private NativeParallelHashMap<Entity, int> fleetMemberCounts;
    private ComponentLookup<LocalToWorld> ltwLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        fleetCohesionMap = new NativeParallelHashMap<Entity, float>(64, Allocator.Persistent);
        fleetMemberCounts = new NativeParallelHashMap<Entity, int>(64, Allocator.Persistent);
        ltwLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        ltwLookup.Update(ref state);

        fleetCohesionMap.Clear();
        fleetMemberCounts.Clear();

        foreach (var (member, memberLtw) in SystemAPI.Query<RefRO<FleetMember>, RefRO<LocalToWorld>>())
        {
            var fleetEntity = member.ValueRO.FleetReference;
            if (!ltwLookup.HasComponent(fleetEntity)) continue;

            var fleetTransform = ltwLookup[fleetEntity];
            float3 expectedPos = math.transform(fleetTransform.Value, member.ValueRO.LocalOffset);
            float actualDistance =math.max(0.0f, math.distance(expectedPos, memberLtw.ValueRO.Position) - FleetConstants.AcceptableDistanceForMaximumCohesion);

            float loss = math.saturate(actualDistance / FleetConstants.DistanceForMaximumCohesionLoss) * FleetConstants.MaximumCohesion;

            fleetCohesionMap.TryGetValue(fleetEntity, out float total);
            fleetCohesionMap[fleetEntity] = total + (FleetConstants.MaximumCohesion - loss);

            fleetMemberCounts.TryGetValue(fleetEntity, out int count);
            fleetMemberCounts[fleetEntity] = count + 1;
        }

        foreach (var kvp in fleetCohesionMap)
        {
            var fleet = kvp.Key;
            var sum = kvp.Value;
            var count = fleetMemberCounts[fleet];

            float avgCohesion = sum / math.max(1, count);

            if (SystemAPI.HasComponent<FleetEntity>(fleet))
            {
                var fleetData = SystemAPI.GetComponent<FleetEntity>(fleet);
                fleetData.Cohesion = avgCohesion;
                var t = math.unlerp(0.0f, FleetConstants.MaximumCohesion, avgCohesion);
                var speedMultiplier = math.lerp(FleetConstants.SpeedMultiplierOnZeroCohesion, 1.0f, t);
                fleetData.CohesionSpeedMultiplier = speedMultiplier;
                SystemAPI.SetComponent(fleet, fleetData);
            }
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        fleetCohesionMap.Dispose();
        fleetMemberCounts.Dispose();
    }
}

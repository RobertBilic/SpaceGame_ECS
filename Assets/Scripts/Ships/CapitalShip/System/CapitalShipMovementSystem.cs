using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct CapitalShipMovementSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<CapitalShipTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.TryGetSingleton<GlobalTimeComponent>(out var timeComp))
            return;

        float deltaTime = timeComp.DeltaTimeScaled; 

        foreach (var (transform, speed, index, entity) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<MoveSpeed>, RefRW<CurrentWaypointIndex>>()
                     .WithEntityAccess())
        {
            var buffer = SystemAPI.GetBuffer<Waypoint>(entity);
            if (buffer.Length == 0 || index.ValueRO.Value >= buffer.Length) return;

            float3 target = buffer[index.ValueRO.Value].Position;
            float3 direction = math.normalize(target - transform.ValueRO.Position);
            float distance = math.distance(transform.ValueRO.Position, target);

            float move = speed.ValueRO.Value * deltaTime;

            if (move >= distance)
            {
                // Reached waypoint, go to next
                transform.ValueRW.Position = target;
                index.ValueRW.Value += 1;
            }
            else
            {
                transform.ValueRW.Position += direction * move; 
            }

            float angle = math.atan2(direction.y, direction.x);
            transform.ValueRW.Rotation = quaternion.RotateZ(angle);
        }
    }
}

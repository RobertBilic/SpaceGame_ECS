using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using Unity.Burst;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class ImpactSpawnSystem : SystemBase
{
    private Unity.Mathematics.Random random;

    protected override void OnCreate()
    {
        RequireForUpdate<ImpactSpawnRequest>();
        random = Unity.Mathematics.Random.CreateFromIndex((uint)SystemAPI.Time.ElapsedTime);
    }

    protected override void OnUpdate()
    {
        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (request, entity) in SystemAPI.Query<RefRO<ImpactSpawnRequest>>().WithEntityAccess())
        {
            for (int i = 0; i < request.ValueRO.Count; i++)
            {
                float3 randomDir = math.normalize(
                    request.ValueRO.Normal + random.NextFloat3Direction() * 0.5f);

                float speed = random.NextFloat(2f, 5f);
                float3 velocity = randomDir * speed;

                var particle = commandBuffer.Instantiate(request.ValueRO.Prefab);
                var scale = request.ValueRO.Scale;
                commandBuffer.SetComponent(particle, new LocalTransform
                {
                    Position = request.ValueRO.Position,
                    Rotation = random.NextQuaternionRotation(),
                    Scale = random.NextFloat(scale / 2, scale)
                }); ;
                commandBuffer.AddComponent(particle, new ImpactParticle
                {
                    Lifetime = 0.5f,
                    Age = 0f,
                    Velocity = velocity
                });
            }

            commandBuffer.DestroyEntity(entity);
        }

        commandBuffer.Playback(EntityManager);
        commandBuffer.Dispose();
    }
}

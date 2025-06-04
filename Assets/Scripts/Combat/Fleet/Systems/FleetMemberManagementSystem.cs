using SpaceGame.Combat.Components;
using SpaceGame.Detection.Component;
using SpaceGame.SpatialGrid.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace SpaceGame.Combat.Fleets
{
    [UpdateInGroup(typeof(CombatInitializationGroup))]
    public partial struct FleetMemberManagementSystem : ISystem
    {

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<FleetEntity>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Persistent);

            foreach (var (fleetMemberBuffer, fleetEntity, entity) in SystemAPI.Query<DynamicBuffer<FleetMemberReference>, RefRW<FleetEntity>>()
                .WithEntityAccess())
            {
                if (fleetEntity.ValueRO.Leader != Entity.Null && state.EntityManager.Exists(fleetEntity.ValueRO.Leader))
                    continue;

                for (int i = fleetMemberBuffer.Length - 1; i >= 0; i--)
                {
                    if(fleetMemberBuffer[i].Ref == Entity.Null || !state.EntityManager.Exists(fleetMemberBuffer[i].Ref))
                        fleetMemberBuffer.RemoveAt(i);
                }

                if(fleetMemberBuffer.Length == 0)
                {
                    ecb.DestroyEntity(entity);
                    continue;
                }

                var newLeader = fleetMemberBuffer[0].Ref;
                ecb.RemoveComponent<FleetMember>(newLeader);
                ecb.AddComponent(newLeader, new FleetLeader() { FleetReference = newLeader });
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }
    }
}

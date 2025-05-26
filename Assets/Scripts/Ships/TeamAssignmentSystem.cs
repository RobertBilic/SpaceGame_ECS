using SpaceGame.Combat.Components;
using Unity.Burst;
using Unity.Entities;

namespace SpaceGame.Combat.Systems
{
    [UpdateInGroup(typeof(CombatInitializationGroup))]
    [UpdateAfter(typeof(ShipConstructionSystem))]
    partial struct TeamAssignmentSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NeedsTeamAssignment>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            foreach (var (turretBuffer, weaponBuffer, teamTag, entity) in SystemAPI.Query<DynamicBuffer<TurretElement>, DynamicBuffer<ForwardWeaponElement>, RefRO<TeamTag>>()
                .WithAll<NeedsTeamAssignment>()
                .WithEntityAccess())
            {
                foreach (var turret in turretBuffer)
                    ecb.AddComponent(turret.Ref, new TeamTag() { Team = teamTag.ValueRO.Team });
                foreach (var weaponElement in weaponBuffer)
                    ecb.AddComponent(weaponElement.Ref, new TeamTag() { Team = teamTag.ValueRO.Team });

                ecb.RemoveComponent<NeedsTeamAssignment>(entity);
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
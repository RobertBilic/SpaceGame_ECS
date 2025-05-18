using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using SpaceGame.SpatialGrid.Components;
using SpaceGame.Combat.Components;

namespace SpaceGame.Combat.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(CombatInitializationGroup))]
    public partial struct CapitalShipConstructionSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            foreach (var (request, requestEntity) in SystemAPI.Query<RefRO<CapitalShipConstructionRequest>>().WithEntityAccess())
            {
                Entity capitalShip = ecb.Instantiate(request.ValueRO.CapitalShipPrefab);

                var capitalShipTransform = new LocalTransform
                {
                    Position = request.ValueRO.SpawnPosition,
                    Rotation = quaternion.identity,
                    Scale = request.ValueRO.Scale
                };

                ecb.AddComponent(capitalShip, capitalShipTransform);
                ecb.AddComponent(capitalShip, new TeamTag() { Team = 1 });
                ecb.AddComponent(capitalShip, new IsAlive() { });
                ecb.AddComponent(capitalShip, new SceneMovementData() { Value = 100.0f });
                ecb.AddComponent(capitalShip, new TargetableTag());
                ecb.AddBuffer<SpatialDatabaseCellIndex>(capitalShip);
                ecb.AddComponent(capitalShip, new Health() { Current = 999999999, Max = 999999999 });
                ecb.AddBuffer<DamageHealthRequestBuffer>(capitalShip);

                var waypointBuffer = ecb.AddBuffer<Waypoint>(capitalShip);
                waypointBuffer.Add(new Waypoint() { Position = new float3(10, 0, 0) });

                ecb.AddComponent(capitalShip, new CurrentWaypointIndex() { Value = 0 });

                if (state.EntityManager.HasBuffer<CapitalShipTurret>(requestEntity))
                {
                    var turretBuffer = SystemAPI.GetBuffer<CapitalShipTurret>(requestEntity);

                    for (int i = 0; i < turretBuffer.Length; i++)
                    {
                        Entity turret = ecb.Instantiate(turretBuffer[i].TurretPrefab);
                        var turretData = turretBuffer[i];

                        ecb.AddComponent(turret, new Parent
                        {
                            Value = capitalShip
                        });

                        ecb.AddComponent(turret, new LocalTransform
                        {
                            Position = turretBuffer[i].Position,
                            Rotation = quaternion.identity,
                            Scale = turretBuffer[i].Scale
                        });

                        ecb.AddComponent(turret, new TeamTag() { Team = 1 });
                        ecb.AppendToBuffer<LinkedEntityGroup>(capitalShip, new LinkedEntityGroup()
                        {
                            Value = turret
                        });
                    }
                }

                ecb.DestroyEntity(requestEntity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
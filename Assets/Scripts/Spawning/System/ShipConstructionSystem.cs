using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using SpaceGame.SpatialGrid.Components;
using SpaceGame.Combat.Components;
using SpaceGame.Movement.Components;

namespace SpaceGame.Combat.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(CombatInitializationGroup))]
    public partial struct ShipConstructionSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<ShipPrefabLookupSingleton>(out var prefabSingleton))
                return;

            ref var lookup = ref prefabSingleton.Lookup.Value;

            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            foreach (var (request, requestEntity) in SystemAPI.Query<RefRO<ShipConstructionRequest>>().WithEntityAccess())
            {
                var prefab = lookup.GetPrefab(request.ValueRO.Id);

                if (prefab.Value == Entity.Null)
                    continue;

                Entity shipEntity = ecb.Instantiate(prefab.Value);

                var capitalShipTransform = new LocalTransform
                {
                    Position = request.ValueRO.SpawnPosition,
                    Rotation = quaternion.identity,
                    Scale = prefab.Scale
                };

                ecb.AddComponent(shipEntity, capitalShipTransform);
                ecb.AddComponent(shipEntity, new TeamTag() { Team = request.ValueRO.Team });
                ecb.AddComponent(shipEntity, new IsAlive() { });
                ecb.AddComponent(shipEntity, new TargetableTag());
                ecb.AddComponent(shipEntity, new CombatStateEntity());
                ecb.AddComponent(shipEntity, new NeedsCombatStateChange());
                ecb.AddComponent(shipEntity, new DesiredMovementDistance());
                ecb.AddComponent(shipEntity, new DesiredSpeed());
                ecb.AddComponent(shipEntity, new DesiredMovementDirection());
                ecb.AddComponent(shipEntity, new NeedsBoundingRadiusScalingTag());

                ecb.AddBuffer<SpatialDatabaseCellIndex>(shipEntity);
                ecb.AddBuffer<DamageHealthRequestBuffer>(shipEntity);

                if (state.EntityManager.HasBuffer<ShipConstructionAddonRequest>(requestEntity))
                {
                    var addons = state.EntityManager.GetBuffer<ShipConstructionAddonRequest>(requestEntity);

                    foreach (var addon in addons)
                        ecb.AddComponent(shipEntity, addon.ComponentToAdd);
                }

                if (state.EntityManager.HasBuffer<ShipTurretConstructionRequest>(requestEntity))
                {
                    var turretConstructionRequestBuffer = state.EntityManager.GetBuffer<ShipTurretConstructionRequest>(requestEntity, true);

                    foreach(var turretConstructionRequest in turretConstructionRequestBuffer)
                    {
                        var turretConstructionRequestEntity = ecb.CreateEntity();

                        ecb.AddComponent(turretConstructionRequestEntity, new TurretConstructionRequest()
                        {
                            Position = turretConstructionRequest.Position,
                            Scale = turretConstructionRequest.Scale,
                            Id = turretConstructionRequest.Id,
                            RootEntity = shipEntity,
                            Team = request.ValueRO.Team
                        });
                    }
                }

                //TODO: Dynamic Forward Weapons
                if (state.EntityManager.HasBuffer<ShipForwardWeaponConstructionRequest>(requestEntity))
                {
                    var turretConstructionRequestBuffer = state.EntityManager.GetBuffer<ShipTurretConstructionRequest>(requestEntity, true);

                    foreach (var turretConstructionRequest in turretConstructionRequestBuffer)
                    {

                    }
                }

                ecb.DestroyEntity(requestEntity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
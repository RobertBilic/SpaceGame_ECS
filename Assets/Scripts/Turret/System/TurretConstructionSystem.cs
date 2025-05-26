using SpaceGame.Combat.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace SpaceGame.Combat.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(CombatInitializationGroup))]
    [UpdateAfter(typeof(ShipConstructionSystem))]
    public partial struct TurretConstructionSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {

        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<TurretPrefabLookupSingleton>(out var prefabSingleton))
                return;

            ref var lookup = ref prefabSingleton.Lookup.Value;

            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            foreach(var (turretConstructionRequest,entity) in SystemAPI.Query<RefRO<TurretConstructionRequest>>()
                .WithEntityAccess())
            {
                var request = turretConstructionRequest.ValueRO;

                var prefab = lookup.GetPrefab(request.Id);
                var turret = ecb.Instantiate(prefab.PrefabEntity);

                if(request.RootEntity != Entity.Null)
                {
                    ecb.AddComponent(turret, new Parent
                    {
                        Value = request.RootEntity
                    });

                    ecb.AppendToBuffer<LinkedEntityGroup>(request.RootEntity, new LinkedEntityGroup()
                    {
                        Value = turret
                    });
                    ecb.AppendToBuffer<TurretElement>(request.RootEntity, new TurretElement() { Ref = turret });
                }

                ecb.AddComponent<LocalTransform>(turret, new LocalTransform()
                {
                    Position = request.Position,
                    Scale = request.Scale,
                    Rotation = Unity.Mathematics.quaternion.identity
                });

                ecb.AddComponent(turret, new TeamTag() { Team = request.Team} );

                ecb.DestroyEntity(entity);
            }

            if (ecb.ShouldPlayback)
                ecb.Playback(state.EntityManager);

            ecb.Dispose();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}

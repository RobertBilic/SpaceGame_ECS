using SpaceGame.Combat.Components;
using SpaceGame.Movement.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

partial struct LevelStartSystem : ISystem
{
    Random random;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        random = new Random(3333444);
        state.RequireForUpdate<LevelStartRequestTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.TryGetSingletonEntity<LevelStartRequestTag>(out var levelStartRequestEntity))
            return;

        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        int playerTeam = 1;

        if (state.EntityManager.HasComponent<PlayerTeam>(levelStartRequestEntity))
            playerTeam = state.EntityManager.GetComponentData<PlayerTeam>(levelStartRequestEntity).Value;

        if (state.EntityManager.HasBuffer<LevelShipEntry>(levelStartRequestEntity))
        {
            var shipBuffer = state.EntityManager.GetBuffer<LevelShipEntry>(levelStartRequestEntity);

            foreach (var ship in shipBuffer)
            {
                for (int i = 0; i < ship.Count; i++)
                {
                    var constructionRequest = ecb.CreateEntity();

                    //TODO: Dynamic weapons for NPCs?

                    var range = new float2(ship.SpawnRadius, ship.SpawnRadius); 
                    var spawnPosition = ship.Position + new float3(random.NextFloat2(-range,range), 0.0f);

                    ecb.AddComponent(constructionRequest, new ShipConstructionRequest()
                    {
                        Id = ship.Id,
                        SpawnPosition = spawnPosition,
                        Team = ship.Team,
                    });

                    var additionalComponentBuffer = ecb.AddBuffer<ShipConstructionAddonRequest>(constructionRequest);

                    if (playerTeam != ship.Team)
                        additionalComponentBuffer.Add(new ShipConstructionAddonRequest() { ComponentToAdd = ComponentType.ReadOnly<EnemyTag>() });
                }
            }

            ecb.DestroyEntity(levelStartRequestEntity);
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}

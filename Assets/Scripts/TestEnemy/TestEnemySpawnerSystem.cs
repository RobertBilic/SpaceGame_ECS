using SpaceGame.Combat.Components;
using SpaceGame.Movement.Flowfield.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SpaceGame.Utility.Temp
{
    [UpdateInGroup(typeof(CombatInitializationGroup))]
    partial struct TestEnemySpawnerSystem : ISystem
    {
        private bool isInitialized;
        private EntityQuery enemyQuery;
        private int desiredEnemies;
        Random random;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            enemyQuery = state.GetEntityQuery(ComponentType.ReadOnly<EnemyTag>());
            desiredEnemies = 50;
            random = new Random(32323);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            var spawnedEnemiesCount = enemyQuery.CalculateEntityCount();
            int enemiesToSpawn = desiredEnemies - spawnedEnemiesCount;

            for (int i = 0; i < enemiesToSpawn; i++)
            {
                var spawnedEntity = ecb.CreateEntity();

                float x = random.NextFloat(-100, 100);
                float y = random.NextFloat(-100, 100);

                ecb.AddComponent(spawnedEntity, new ShipConstructionRequest()
                {
                    Id = "ships_test_enemy",
                    Scale = 1.0f,
                    SpawnPosition = new float3(x, y, 0.0f),
                    Team = 2,
                });

                var addonRequests = ecb.AddBuffer<ShipConstructionAddonRequest>(spawnedEntity);

                addonRequests.Add(new ShipConstructionAddonRequest() { ComponentToAdd = ComponentType.ReadOnly<EnemyTag>() });
                addonRequests.Add(new ShipConstructionAddonRequest() { ComponentToAdd = ComponentType.ReadOnly<FlowFieldMovementEntityTag>() });
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
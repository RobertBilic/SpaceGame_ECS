using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct TestEnemySpawnerSystem : ISystem
{
    private Entity enemyPrefab;
    private bool isInitialized;
    private EntityQuery enemyQuery;
    private int desiredEnemies;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        enemyQuery = state.GetEntityQuery(ComponentType.ReadOnly<Team2Tag>());
        desiredEnemies = 100;
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach (var (prefab, generator) in SystemAPI.Query<RefRO<TestEnemyPrefab>, RefRW<RandomGenerator>>().WithOptions(EntityQueryOptions.IncludePrefab))
        {
            isInitialized = true;
            enemyPrefab = prefab.ValueRO.Value;
            var spawnedEnemiesCount = enemyQuery.CalculateEntityCount();
            int enemiesToSpawn = desiredEnemies - spawnedEnemiesCount;

            for (int i = 0; i < enemiesToSpawn; i++)
            {
                var spawnedEntity = ecb.Instantiate(enemyPrefab);

                float x = generator.ValueRW.Value.NextFloat(-100, 100);
                float y = generator.ValueRW.Value.NextFloat(-100, 100);

                ecb.AddComponent(spawnedEntity, new LocalTransform()
                {
                    Position = new Unity.Mathematics.float3(x, y, 0.0f),
                    Rotation = quaternion.identity,
                    Scale = 1
                });

            }
        }

        if (!isInitialized)
            return;

        

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}

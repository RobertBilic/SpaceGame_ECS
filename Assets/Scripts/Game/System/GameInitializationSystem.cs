using SpaceGame.Combat.Components;
using SpaceGame.SpatialGrid.Components;
using Unity.Burst;
using Unity.Entities;

namespace SpaceGame.Game.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    partial struct GameInitializationSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Config>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.TryGetSingleton(out Config config))
            {
                if (config.IsInitialized)
                    return;

                // Set state to initialized
                config.IsInitialized = true;
                SystemAPI.SetSingleton(config);

                float simulationCubeHalfExtents = config.GameSize;
                state.EntityManager.AddComponentData(state.EntityManager.CreateEntity(), new SpatialDatabaseSingleton());
                state.EntityManager.CreateSingletonBuffer<BulletSpawnRequest>("Bullet Spawn Request Collector");
                state.EntityManager.CreateSingletonBuffer<BulletPoolRequest>("Bullet Pool Request Collector");
                state.EntityManager.CreateSingletonBuffer<ImpactSpawnRequest>("Impact Effect Spawn Request Collector");
                state.EntityManager.CreateSingletonBuffer<ImpactEffectPoolRequest>("Impact Effect Pool Request Collector");

                var entity = state.EntityManager.CreateSingleton<GlobalTimeComponent>("TimeScaleSingleton");
                state.EntityManager.SetComponentData(entity, new GlobalTimeComponent()
                {
                    FrameCount = 0,
                    ElapsedTime = 0,
                    ElapsedTimeScaled = 0,
                    FrameCountScaled = 0
                });

                CreateTargetablesSpatialDatabase(ref state, in config, simulationCubeHalfExtents);
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }

        private void CreateTargetablesSpatialDatabase(
            ref SystemState state,
            in Config config,
            float simulationCubeHalfExtents)
        {
            ref SpatialDatabaseSingleton spatialDatabaseSingleton = ref SystemAPI.GetSingletonRW<SpatialDatabaseSingleton>().ValueRW;
            spatialDatabaseSingleton.TargetablesSpatialDatabase =
                state.EntityManager.Instantiate(config.SpatialDatabasePrefab);
            SpatialDatabase spatialDatabase =
                state.EntityManager.GetComponentData<SpatialDatabase>(spatialDatabaseSingleton
                    .TargetablesSpatialDatabase);
            DynamicBuffer<SpatialDatabaseCell> cellsBuffer =
                state.EntityManager.GetBuffer<SpatialDatabaseCell>(spatialDatabaseSingleton.TargetablesSpatialDatabase);
            DynamicBuffer<SpatialDatabaseElement> elementsBuffer =
                state.EntityManager.GetBuffer<SpatialDatabaseElement>(spatialDatabaseSingleton
                    .TargetablesSpatialDatabase);

            SpatialDatabase.Initialize(
                simulationCubeHalfExtents,
                config.SpatialDatabaseSubdivisions,
                config.ShipsSpatialDatabaseCellCapacity,
                ref spatialDatabase,
                ref cellsBuffer,
                ref elementsBuffer);

            state.EntityManager.SetComponentData(spatialDatabaseSingleton.TargetablesSpatialDatabase, spatialDatabase);
        }
    }
}
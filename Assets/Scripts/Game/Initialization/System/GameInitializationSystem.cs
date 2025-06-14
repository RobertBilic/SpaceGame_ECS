using SpaceGame.Combat.Components;
using SpaceGame.Game.Initialization.Components;
using SpaceGame.Game.State.Component;
using SpaceGame.SpatialGrid.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace SpaceGame.Game.Initialization.Systems 
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    partial struct GameInitializationSystem : ISystem
    {
        BlobAssetReference<TeamSpatialDatabaseLookup> blobRef;

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
                var spatialDatabaseSingletonEntity = state.EntityManager.CreateEntity();

                var builder = new BlobBuilder(Allocator.Temp);
                ref var root = ref builder.ConstructRoot<TeamSpatialDatabaseLookup>();
                var array = builder.Allocate(ref root.TeamBasedDatabases, config.TeamCount);
                for (int i = 0; i < config.TeamCount; i++)
                    CreateTeamBasedSpatialDatabase(ref state, ref array, in config, simulationCubeHalfExtents, i, i);
                blobRef = builder.CreateBlobAssetReference<TeamSpatialDatabaseLookup>(Allocator.Persistent);
                builder.Dispose();
                var spatialDatabaseSingleton = new SpatialDatabaseSingleton()
                {
                    AllTargetablesDatabase = Entity.Null,
                    TeamBasedDatabases = blobRef
                };

                state.EntityManager.AddComponentData(spatialDatabaseSingletonEntity, spatialDatabaseSingleton);
                state.EntityManager.CreateSingletonBuffer<ProjectileSpawnRequest>("Bullet Spawn Request Collector");
                state.EntityManager.CreateSingletonBuffer<ProjectilePoolRequest>("Bullet Pool Request Collector");
                state.EntityManager.CreateSingletonBuffer<ImpactSpawnRequest>("Impact Effect Spawn Request Collector");
                state.EntityManager.CreateSingletonBuffer<ImpactEffectPoolRequest>("Impact Effect Pool Request Collector");
                state.EntityManager.CreateSingleton(new GameStateComponent() { Value = GameState.MainMenu }, "GameState");
                state.EntityManager.CreateSingleton(new ChangeGameStateRequest() { Value = GameState.MainMenu });
                state.EntityManager.CreateSingleton<GameInitializedTag>();


                var entity = state.EntityManager.CreateSingleton<GlobalTimeComponent>("TimeScaleSingleton");
                state.EntityManager.SetComponentData(entity, new GlobalTimeComponent()
                {
                    FrameCount = 0,
                    ElapsedTime = 0,
                    ElapsedTimeScaled = 0,
                    FrameCountScaled = 0
                });

                CreateTrailSpatialDatabase(ref state, in config, simulationCubeHalfExtents);
                CreateTargetablesSpatialDatabase(ref state, in config, simulationCubeHalfExtents);

            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            blobRef.Dispose();
        }

        private void CreateTrailSpatialDatabase(ref SystemState state, in Config config, float simulationCubeHalfExtents)
        {
            ref SpatialDatabaseSingleton spatialDatabaseSingleton = ref SystemAPI.GetSingletonRW<SpatialDatabaseSingleton>().ValueRW;
            spatialDatabaseSingleton.TrailDatabase = state.EntityManager.Instantiate(config.SpatialDatabasePrefab);
            SpatialDatabase spatialDatabase = state.EntityManager.GetComponentData<SpatialDatabase>(spatialDatabaseSingleton.TrailDatabase);
            DynamicBuffer<SpatialDatabaseCell> cellsBuffer = state.EntityManager.GetBuffer<SpatialDatabaseCell>(spatialDatabaseSingleton.TrailDatabase);
            DynamicBuffer<SpatialDatabaseElement> elementsBuffer = state.EntityManager.GetBuffer<SpatialDatabaseElement>(spatialDatabaseSingleton.TrailDatabase);

            SpatialDatabase.Initialize(
                simulationCubeHalfExtents,
                config.SpatialDatabaseSubdivisions,
                config.ShipsSpatialDatabaseCellCapacity,
                ref spatialDatabase,
                ref cellsBuffer,
                ref elementsBuffer);

            state.EntityManager.SetComponentData(spatialDatabaseSingleton.TrailDatabase, spatialDatabase);
        }

        private void CreateTargetablesSpatialDatabase(ref SystemState state, in Config config, float simulationCubeHalfExtents)
        {
            ref SpatialDatabaseSingleton spatialDatabaseSingleton = ref SystemAPI.GetSingletonRW<SpatialDatabaseSingleton>().ValueRW;
            spatialDatabaseSingleton.AllTargetablesDatabase = state.EntityManager.Instantiate(config.SpatialDatabasePrefab);
            SpatialDatabase spatialDatabase = state.EntityManager.GetComponentData<SpatialDatabase>(spatialDatabaseSingleton.AllTargetablesDatabase);
            DynamicBuffer<SpatialDatabaseCell> cellsBuffer = state.EntityManager.GetBuffer<SpatialDatabaseCell>(spatialDatabaseSingleton.AllTargetablesDatabase);
            DynamicBuffer<SpatialDatabaseElement> elementsBuffer = state.EntityManager.GetBuffer<SpatialDatabaseElement>(spatialDatabaseSingleton.AllTargetablesDatabase);

            SpatialDatabase.Initialize(
                simulationCubeHalfExtents,
                config.SpatialDatabaseSubdivisions,
                config.ShipsSpatialDatabaseCellCapacity,
                ref spatialDatabase,
                ref cellsBuffer,
                ref elementsBuffer);

            state.EntityManager.SetComponentData(spatialDatabaseSingleton.AllTargetablesDatabase, spatialDatabase);
        }

        private void CreateTeamBasedSpatialDatabase(ref SystemState state, ref BlobBuilderArray<TeamSpatialDatabase> array,in Config config, float simulationCubeHalfExtents, int index, int team)
        { 
            var database = new TeamSpatialDatabase()
            {
                Database = state.EntityManager.Instantiate(config.SpatialDatabasePrefab),
                Team = team
            };

            SpatialDatabase spatialDatabase = state.EntityManager.GetComponentData<SpatialDatabase>(database.Database);
            DynamicBuffer<SpatialDatabaseCell> cellsBuffer = state.EntityManager.GetBuffer<SpatialDatabaseCell>(database.Database);
            DynamicBuffer<SpatialDatabaseElement> elementsBuffer = state.EntityManager.GetBuffer<SpatialDatabaseElement>(database.Database);

            SpatialDatabase.Initialize(
                simulationCubeHalfExtents,
                config.SpatialDatabaseSubdivisions,
                config.ShipsSpatialDatabaseCellCapacity,
                ref spatialDatabase,
                ref cellsBuffer,
                ref elementsBuffer);

            state.EntityManager.SetComponentData(database.Database, spatialDatabase);
            array[index] = database;
        }
    }
}
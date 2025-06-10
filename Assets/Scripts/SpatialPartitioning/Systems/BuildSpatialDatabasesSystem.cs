using SpaceGame.SpatialGrid.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;

namespace SpaceGame.SpatialGrid.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(BuildSpatialDatabaseGroup))]
    public partial struct BuildSpatialDatabasesSystem : ISystem
    {
        private EntityQuery _spatialDatabasesQuery;
        private BufferLookup<SpatialDatabaseCellIndex> _cellIndexBufferLookUpRW;
        private BufferLookup<HitBoxElement> hitboxBufferLookup;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _spatialDatabasesQuery = SystemAPI.QueryBuilder().WithAll<SpatialDatabase, SpatialDatabaseCell, SpatialDatabaseElement>().Build();
            _cellIndexBufferLookUpRW = SystemAPI.GetBufferLookup<SpatialDatabaseCellIndex>(false);
            hitboxBufferLookup = SystemAPI.GetBufferLookup<HitBoxElement>(true);

            state.RequireForUpdate<SpatialDatabaseSingleton>();
            state.RequireForUpdate(_spatialDatabasesQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            SpatialDatabaseSingleton spatialDatabaseSingleton = SystemAPI.GetSingleton<SpatialDatabaseSingleton>();
            _cellIndexBufferLookUpRW.Update(ref state);
            hitboxBufferLookup.Update(ref state);

            ref var teamBasedSpatialDatabases = ref spatialDatabaseSingleton.TeamBasedDatabases.Value.TeamBasedDatabases;

            var grid = SystemAPI.GetComponentRO<SpatialDatabase>(spatialDatabaseSingleton.AllTargetablesDatabase).ValueRO.Grid;
            SpatialDatabaseParallelComputeCellIndexJob cellIndexJob = new SpatialDatabaseParallelComputeCellIndexJob
            {
                Grid = grid,
                HitBoxBufferLookup = hitboxBufferLookup,
                CellIndexBufferLookup = _cellIndexBufferLookUpRW
            };

            state.Dependency = cellIndexJob.ScheduleParallel(state.Dependency);
            ConstructDatabase(ref state, spatialDatabaseSingleton.AllTargetablesDatabase, -1);
            for (int i = 0; i < teamBasedSpatialDatabases.Length; i++)
                ConstructDatabase(ref state, teamBasedSpatialDatabases[i].Database, teamBasedSpatialDatabases[i].Team);

            state.Dependency.Complete();
        }

        [BurstCompile]
        public void ConstructDatabase(ref SystemState state, Entity database, int team)
        {

            CachedSpatialDatabaseUnsafe cachedSpatialDatabase = new CachedSpatialDatabaseUnsafe
            {
                SpatialDatabaseEntity = database,
                SpatialDatabaseLookup = SystemAPI.GetComponentLookup<SpatialDatabase>(false),
                CellsBufferLookup = SystemAPI.GetBufferLookup<SpatialDatabaseCell>(false),
                ElementsBufferLookup = SystemAPI.GetBufferLookup<SpatialDatabaseElement>(false),
                Team = team
            };

            // Launch X jobs, each responsible for 1/Xth of spatial database cells
            JobHandle initialDep = state.Dependency;
            int parallelCount = math.max(1, 1);
            for (int s = 0; s < parallelCount; s++)
            {
                BuildSpatialDatabaseParallelJob buildJob = new BuildSpatialDatabaseParallelJob
                {
                    JobSequenceNb = s,
                    JobsTotalCount = parallelCount,
                    CachedSpatialDatabase = cachedSpatialDatabase,
                    CellIndexBufferLookup = _cellIndexBufferLookUpRW
                };
                state.Dependency = JobHandle.CombineDependencies(state.Dependency, buildJob.ScheduleParallel(initialDep));
            }

        }
    }
}
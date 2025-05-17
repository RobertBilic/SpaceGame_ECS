using SpaceGame.Combat.Components;
using SpaceGame.SpatialGrid.Components;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace SpaceGame.SpatialGrid.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(BuildSpatialDatabaseGroup))]
    public partial struct BuildSpatialDatabasesSystem : ISystem
    {
        private EntityQuery _spatialDatabasesQuery;
        private BufferLookup<SpatialDatabaseCellIndex> _cellIndexBufferLookUpRW;
        private BufferLookup<SpatialDatabaseCellIndex> _cellIndexBufferLookUpRO;
        private BufferLookup<HitBoxElement> hitboxBufferLookup;
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _spatialDatabasesQuery = SystemAPI.QueryBuilder().WithAll<SpatialDatabase, SpatialDatabaseCell, SpatialDatabaseElement>().Build();
            _cellIndexBufferLookUpRW = SystemAPI.GetBufferLookup<SpatialDatabaseCellIndex>(false);
            _cellIndexBufferLookUpRO = SystemAPI.GetBufferLookup<SpatialDatabaseCellIndex>(true);
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

            CachedSpatialDatabaseUnsafe cachedSpatialDatabase = new CachedSpatialDatabaseUnsafe
            {
                SpatialDatabaseEntity = spatialDatabaseSingleton.TargetablesSpatialDatabase,
                SpatialDatabaseLookup = SystemAPI.GetComponentLookup<SpatialDatabase>(false),
                CellsBufferLookup = SystemAPI.GetBufferLookup<SpatialDatabaseCell>(false),
                ElementsBufferLookup = SystemAPI.GetBufferLookup<SpatialDatabaseElement>(false),
            };


            // Make each ship calculate the octant it belongs to
            SpatialDatabaseParallelComputeCellIndexJob cellIndexJob = new SpatialDatabaseParallelComputeCellIndexJob
            {
                CachedSpatialDatabase = cachedSpatialDatabase,
                HitBoxBufferLookup = hitboxBufferLookup,
                CellIndexBufferLookup = _cellIndexBufferLookUpRW
            };

            state.Dependency = cellIndexJob.ScheduleParallel(state.Dependency);

            _cellIndexBufferLookUpRO.Update(ref state);
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

            state.Dependency.Complete();
        }

        [BurstCompile]
        [WithAll(typeof(TargetableTag))]
        public partial struct BuildSpatialDatabaseSingleJob : IJobEntity, IJobEntityChunkBeginEnd
        {
            public CachedSpatialDatabase CachedSpatialDatabase;

            public void Execute(Entity entity, in LocalToWorld ltw, in TeamTag team)
            {
                SpatialDatabaseElement element = new SpatialDatabaseElement
                {
                    Entity = entity,
                    Position = ltw.Position,
                    Team = (byte)team.Team,
                };
                SpatialDatabase.AddToDataBase(in CachedSpatialDatabase._SpatialDatabase,
                    ref CachedSpatialDatabase._SpatialDatabaseCells, ref CachedSpatialDatabase._SpatialDatabaseElements,
                    element);
            }

            public bool OnChunkBegin(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                CachedSpatialDatabase.CacheData();
                return true;
            }

            public void OnChunkEnd(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask,
                bool chunkWasExecuted)
            {
            }
        }

        [BurstCompile]
        public partial struct SpatialDatabaseParallelComputeCellIndexJob : IJobEntity, IJobEntityChunkBeginEnd
        {
            public CachedSpatialDatabaseUnsafe CachedSpatialDatabase;

            // other cached data
            private UniformOriginGrid _grid;

            [NativeDisableParallelForRestriction]
            public BufferLookup<SpatialDatabaseCellIndex> CellIndexBufferLookup;
            [ReadOnly]
            public BufferLookup<HitBoxElement> HitBoxBufferLookup;

            public void Execute(Entity entity, in LocalToWorld ltw)
            {
                if (!CellIndexBufferLookup.HasBuffer(entity)) return;
                if (!HitBoxBufferLookup.HasBuffer(entity)) return;

                var cellIndexBuffer = CellIndexBufferLookup[entity];
                cellIndexBuffer.Clear();

                var hitboxes = HitBoxBufferLookup[entity];

                float3 position = ltw.Position;

                foreach (var hitbox in hitboxes)
                {
                    float3 halfExtents = hitbox.HalfExtents * ltw.Value.Scale();
                    float3 finalPos = position + hitbox.LocalCenter * ltw.Value.Scale();

                    float3 min = finalPos - halfExtents;
                    float3 max = finalPos + halfExtents;

                    if (UniformOriginGrid.GetAABBMinMaxCoords(in _grid, min, max, out int3 minCoords, out int3 maxCoords))
                    {
                        for (int z = minCoords.z; z <= maxCoords.z; z++)
                            for (int y = minCoords.y; y <= maxCoords.y; y++)
                                for (int x = minCoords.x; x <= maxCoords.x; x++)
                                {
                                    int cellIndex = UniformOriginGrid.GetCellIndexFromCoords(_grid, new int3(x, y, z));

                                    bool alreadyAdded = false;

                                    for (int i = 0; i < cellIndexBuffer.Length; i++)
                                    {
                                        if (cellIndexBuffer[i].CellIndex == cellIndex)
                                        {
                                            alreadyAdded = true;
                                            break;
                                        }
                                    }

                                    if (alreadyAdded)
                                        continue;

                                    cellIndexBuffer.Add(new SpatialDatabaseCellIndex { CellIndex = cellIndex });
                                }
                    }
                }
            }


            public bool OnChunkBegin(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                CachedSpatialDatabase.CacheData();
                _grid = CachedSpatialDatabase._SpatialDatabase.Grid;
                return true;
            }

            public void OnChunkEnd(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask,
                bool chunkWasExecuted)
            {
            }
        }

        [BurstCompile]
        [WithAll(typeof(TargetableTag))]
        public partial struct BuildSpatialDatabaseParallelJob : IJobEntity, IJobEntityChunkBeginEnd
        {
            public int JobSequenceNb;
            public int JobsTotalCount;
            public CachedSpatialDatabaseUnsafe CachedSpatialDatabase;

            [ReadOnly]
            [NativeDisableParallelForRestriction]
            public BufferLookup<SpatialDatabaseCellIndex> CellIndexBufferLookup;
            public void Execute(Entity entity, in LocalToWorld ltw, in TeamTag team)
            {
                if (!CellIndexBufferLookup.HasBuffer(entity))
                    return;

                var buffer = CellIndexBufferLookup[entity];

                for (int i = 0; i < buffer.Length; i++)
                {
                    int cellIndex = buffer[i].CellIndex;

                    if (cellIndex % JobsTotalCount != JobSequenceNb)
                        continue;

                    var element = new SpatialDatabaseElement
                    {
                        Entity = entity,
                        Position = ltw.Position,
                        Team = (byte)team.Team,
                    };

                    SpatialDatabase.AddToDataBase(
                        in CachedSpatialDatabase._SpatialDatabase,
                        ref CachedSpatialDatabase._SpatialDatabaseCells,
                        ref CachedSpatialDatabase._SpatialDatabaseElements,
                        element, cellIndex
                    );
                }
            }


            public bool OnChunkBegin(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                CachedSpatialDatabase.CacheData();
                return true;
            }

            public void OnChunkEnd(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask,
                bool chunkWasExecuted)
            {
            }
        }
    }
}
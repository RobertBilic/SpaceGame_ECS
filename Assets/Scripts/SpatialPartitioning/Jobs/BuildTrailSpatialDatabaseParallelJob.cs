﻿using SpaceGame.Combat.Components;
using SpaceGame.SpatialGrid.Components;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

[BurstCompile]
[WithAll(typeof(TrailRendererTag))]
public partial struct BuildTrailSpatialDatabaseParallelJob : IJobEntity, IJobEntityChunkBeginEnd
{
    public int JobSequenceNb;
    public int JobsTotalCount;
    public CachedSpatialDatabaseUnsafe CachedSpatialDatabase;

    [ReadOnly]
    [NativeDisableParallelForRestriction]
    public BufferLookup<SpatialDatabaseCellIndex> CellIndexBufferLookup;
    public void Execute(Entity entity, in LocalToWorld ltw)
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
                Team = 0
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
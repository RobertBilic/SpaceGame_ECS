using SpaceGame.Combat.Components;
using SpaceGame.SpatialGrid.Components;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Entities;
using Unity.Transforms;

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
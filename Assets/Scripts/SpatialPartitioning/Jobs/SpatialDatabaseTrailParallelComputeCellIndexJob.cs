using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
[WithAll(typeof(TrailRendererTag))]
public partial struct SpatialDatabaseTrailParallelComputeCellIndexJob : IJobEntity
{
    public UniformOriginGrid Grid;

    [NativeDisableParallelForRestriction]
    public BufferLookup<SpatialDatabaseCellIndex> CellIndexBufferLookup;

    public void Execute(Entity entity, in LocalToWorld ltw)
    {
        if (!CellIndexBufferLookup.HasBuffer(entity)) return;

        var cellIndexBuffer = CellIndexBufferLookup[entity];
        cellIndexBuffer.Clear();

        cellIndexBuffer.Add(new SpatialDatabaseCellIndex() { 
            CellIndex = UniformOriginGrid.GetCellIndex(in Grid, ltw.Position)
        });

    }
}
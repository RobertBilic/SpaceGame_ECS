using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct SpatialDatabaseParallelComputeCellIndexJob : IJobEntity
{
    public UniformOriginGrid Grid;

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

            if (UniformOriginGrid.GetAABBMinMaxCoords(in Grid, min, max, out int3 minCoords, out int3 maxCoords))
            {
                for (int z = minCoords.z; z <= maxCoords.z; z++)
                    for (int y = minCoords.y; y <= maxCoords.y; y++)
                        for (int x = minCoords.x; x <= maxCoords.x; x++)
                        {
                            int cellIndex = UniformOriginGrid.GetCellIndexFromCoords(Grid, new int3(x, y, z));

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
}
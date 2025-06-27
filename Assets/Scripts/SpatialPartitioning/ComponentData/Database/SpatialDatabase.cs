using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;


public struct SpatialDatabase : IComponentData
{
    public UniformOriginGrid Grid;

    public const float ElementsCapacityGrowFactor = 2f;

    public static void Initialize(float halfExtents, int subdivisions, int cellEntriesCapacity,
        ref SpatialDatabase spatialDatabase, ref DynamicBuffer<SpatialDatabaseCell> cellsBuffer,
        ref DynamicBuffer<SpatialDatabaseElement> storageBuffer)
    {
        // Clear
        cellsBuffer.Clear();
        storageBuffer.Clear();
        cellsBuffer.Capacity = 16;
        storageBuffer.Capacity = 16;

        spatialDatabase.Grid = new UniformOriginGrid(halfExtents, subdivisions);

        cellsBuffer.Resize(spatialDatabase.Grid.CellCount, NativeArrayOptions.ClearMemory);
        storageBuffer.Resize(spatialDatabase.Grid.CellCount * cellEntriesCapacity, NativeArrayOptions.ClearMemory);

        for (int i = 0; i < cellsBuffer.Length; i++)
        {
            SpatialDatabaseCell cell = cellsBuffer[i];
            cell.StartIndex = i * cellEntriesCapacity;
            cell.ElementsCount = 0;
            cell.ElementsCapacity = cellEntriesCapacity;
            cell.ExcessElementsCount = 0;
            cellsBuffer[i] = cell;
        }
    }

    public static void ClearAndResize(ref DynamicBuffer<SpatialDatabaseCell> cellsBuffer,
        ref DynamicBuffer<SpatialDatabaseElement> storageBuffer)
    {
        int totalDesiredStorage = 0;
        for (int i = 0; i < cellsBuffer.Length; i++)
        {
            SpatialDatabaseCell cell = cellsBuffer[i];
            cell.StartIndex = totalDesiredStorage;

            cell.ElementsCapacity = math.select(cell.ElementsCapacity,
                (int)math.ceil((cell.ElementsCapacity + cell.ExcessElementsCount) * ElementsCapacityGrowFactor),
                cell.ExcessElementsCount > 0);
            totalDesiredStorage += cell.ElementsCapacity;

            // Reset storage
            cell.ElementsCount = 0;
            cell.ExcessElementsCount = 0;

            cellsBuffer[i] = cell;
        }

        storageBuffer.Resize(totalDesiredStorage, NativeArrayOptions.ClearMemory);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddToDataBase(in SpatialDatabase spatialDatabase,
        ref UnsafeList<SpatialDatabaseCell> cellsBuffer, ref UnsafeList<SpatialDatabaseElement> storageBuffer,
        in SpatialDatabaseElement element)
    {
        int cellIndex = UniformOriginGrid.GetCellIndex(in spatialDatabase.Grid, element.Position);
        if (cellIndex >= 0)
        {
            SpatialDatabaseCell cell = cellsBuffer[cellIndex];

            if (cell.ElementsCount + 1 > cell.ElementsCapacity)
            {
                cell.ExcessElementsCount++;
            }
            else
            {
                storageBuffer[cell.StartIndex + cell.ElementsCount] = element;
                cell.ElementsCount++;
            }

            cellsBuffer[cellIndex] = cell;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddToDataBase(in SpatialDatabase spatialDatabase,
        ref UnsafeList<SpatialDatabaseCell> cellsBuffer, ref UnsafeList<SpatialDatabaseElement> storageBuffer,
        in SpatialDatabaseElement element, int cellIndex)
    {
        if (cellIndex >= 0)
        {
            SpatialDatabaseCell cell = cellsBuffer[cellIndex];

            if (cell.ElementsCount + 1 > cell.ElementsCapacity)
            {
                cell.ExcessElementsCount++;
            }
            else
            {
                storageBuffer[cell.StartIndex + cell.ElementsCount] = element;
                cell.ElementsCount++;
            }

            cellsBuffer[cellIndex] = cell;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe static void QueryAABB<T>(in SpatialDatabase spatialDatabase,
        in DynamicBuffer<SpatialDatabaseCell> cellsBuffer, in DynamicBuffer<SpatialDatabaseElement> elementsBuffer,
        float3 center, float3 halfExtents, ref T collector)
        where T : unmanaged, ISpatialQueryCollector
    {
        UnsafeList<SpatialDatabaseCell> cells =
            new UnsafeList<SpatialDatabaseCell>((SpatialDatabaseCell*)cellsBuffer.GetUnsafeReadOnlyPtr(),
                cellsBuffer.Length);
        UnsafeList<SpatialDatabaseElement> elements =
            new UnsafeList<SpatialDatabaseElement>((SpatialDatabaseElement*)elementsBuffer.GetUnsafeReadOnlyPtr(),
                elementsBuffer.Length);
        QueryAABB(in spatialDatabase, in cells, in elements, center, halfExtents, ref collector);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void QueryAABB<T>(in SpatialDatabase spatialDatabase,
        in UnsafeList<SpatialDatabaseCell> cellsBuffer, in UnsafeList<SpatialDatabaseElement> elementsBuffer,
        float3 center, float3 halfExtents, ref T collector)
        where T : unmanaged, ISpatialQueryCollector
    {
        float3 aabbMin = center - halfExtents;
        float3 aabbMax = center + halfExtents;
        UniformOriginGrid grid = spatialDatabase.Grid;
        if (UniformOriginGrid.GetAABBMinMaxCoords(in grid, aabbMin, aabbMax, out int3 minCoords,
                out int3 maxCoords))
        {
            for (int y = minCoords.y; y <= maxCoords.y; y++)
            {
                for (int x = minCoords.x; x <= maxCoords.x; x++)
                {
                    int3 coords = new int3(x, y, 0);
                    int cellIndex = UniformOriginGrid.GetCellIndexFromCoords(in grid, coords);
                    SpatialDatabaseCell cell = cellsBuffer[cellIndex];
                    collector.OnVisitCell(in cell, in elementsBuffer,
                        out bool shouldEarlyExit);
                    if (shouldEarlyExit)
                    {
                        return;
                    }
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe static void QueryAABBCellProximityOrder<T>(in SpatialDatabase spatialDatabase,
        in DynamicBuffer<SpatialDatabaseCell> cellsBuffer, in DynamicBuffer<SpatialDatabaseElement> elementsBuffer,
        float3 center, float3 halfExtents, ref T collector)
        where T : unmanaged, ISpatialQueryCollector
    {
        UnsafeList<SpatialDatabaseCell> cells =
            new UnsafeList<SpatialDatabaseCell>((SpatialDatabaseCell*)cellsBuffer.GetUnsafeReadOnlyPtr(),
                cellsBuffer.Length);
        UnsafeList<SpatialDatabaseElement> elements =
            new UnsafeList<SpatialDatabaseElement>((SpatialDatabaseElement*)elementsBuffer.GetUnsafeReadOnlyPtr(),
                elementsBuffer.Length);
        QueryAABBCellProximityOrder(in spatialDatabase, in cells, in elements, center, halfExtents, ref collector);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void QuerySphereCellProximityOrder<T>(
    in SpatialDatabase spatialDatabase,
    in UnsafeList<SpatialDatabaseCell> cellsBuffer,
    in UnsafeList<SpatialDatabaseElement> elementsBuffer,
    float3 center,
    float radius,
    ref T collector)
    where T : unmanaged, ISpatialQueryCollector
    {
        UniformOriginGrid grid = spatialDatabase.Grid;
        float3 halfExtents = new float3(radius);
        float3 aabbMin = center - halfExtents;
        float3 aabbMax = center + halfExtents;

        if (UniformOriginGrid.GetAABBMinMaxCoords(in grid, aabbMin, aabbMax, out int3 minCoords, out int3 maxCoords))
        {
            int3 sourceCoord = UniformOriginGrid.GetCellCoordsFromPosition(in grid, center);
            int3 highestCoordDistances = math.max(maxCoords - sourceCoord, sourceCoord - minCoords);
            int maxLayer = math.max(highestCoordDistances.x,
                math.max(highestCoordDistances.y, highestCoordDistances.z));

            float radiusSq = radius * radius;

            for (int l = 0; l <= maxLayer; l++)
            {
                int2 yRange = new int2(sourceCoord.y - l, sourceCoord.y + l);
                int2 xRange = new int2(sourceCoord.x - l, sourceCoord.x + l);

                for (int y = yRange.x; y <= yRange.y; y++)
                {
                    for (int x = xRange.x; x <= xRange.y; x++)
                    {
                        int3 coords = new int3(x, y, 0);

                        if (coords.x < minCoords.x || coords.x > maxCoords.x ||
                            coords.y < minCoords.y || coords.y > maxCoords.y)
                            continue;

                        int3 coordDist = math.abs(coords - sourceCoord);

                        int maxCoordsDist = math.max(coordDist.x,
                            math.max(coordDist.y, coordDist.z));

                        if (maxCoordsDist != l)
                        {
                            x = xRange.y - 1;
                            continue;
                        }

                        float3 cellCenter = UniformOriginGrid.GetCellCenter(grid.BoundsMin, grid.CellSize, coords);
                        float3 cellHalfSize = new float3(grid.CellSize * 0.5f);
                        float3 cellMin = cellCenter - cellHalfSize;
                        float3 cellMax = cellCenter + cellHalfSize;

                        if (IntersectAABBWithSphere(cellMin, cellMax, center, radiusSq))
                        {
                            int cellIndex = UniformOriginGrid.GetCellIndexFromCoords(in grid, coords);
                            SpatialDatabaseCell cell = cellsBuffer[cellIndex];

                            collector.OnVisitCell(in cell, in elementsBuffer, out bool shouldExit);
                            if (shouldExit)
                                return;
                        }
                    }
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IntersectAABBWithSphere(float3 aabbMin, float3 aabbMax, float3 sphereCenter, float radiusSq)
    {
        float3 closestPoint = math.clamp(sphereCenter, aabbMin, aabbMax);
        return math.distancesq(closestPoint, sphereCenter) <= radiusSq;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void QueryAABBCellProximityOrder<T>(in SpatialDatabase spatialDatabase,
        in UnsafeList<SpatialDatabaseCell> cellsBuffer, in UnsafeList<SpatialDatabaseElement> elementsBuffer,
        float3 center, float3 halfExtents, ref T collector)
        where T : unmanaged, ISpatialQueryCollector
    {
        float3 aabbMin = center - halfExtents;
        float3 aabbMax = center + halfExtents;
        UniformOriginGrid grid = spatialDatabase.Grid;
        if (UniformOriginGrid.GetAABBMinMaxCoords(in grid, aabbMin, aabbMax, out int3 minCoords, out int3 maxCoords))
        {
            int3 sourceCoord = UniformOriginGrid.GetCellCoordsFromPosition(in grid, center);
            int3 highestCoordDistances = math.max(maxCoords - sourceCoord, sourceCoord - minCoords);
            int maxLayer = math.max(highestCoordDistances.x,
                math.max(highestCoordDistances.y, highestCoordDistances.z));

            for (int l = 0; l <= maxLayer; l++)
            {
                int2 yRange = new int2(sourceCoord.y - l, sourceCoord.y + l);
                int2 xRange = new int2(sourceCoord.x - l, sourceCoord.x + l);

                for (int y = yRange.x; y <= yRange.y; y++)
                {
                    int yDistToEdge = math.min(y - minCoords.y, maxCoords.y - y);

                    if (yDistToEdge < 0)
                    {
                        continue;
                    }


                    for (int x = xRange.x; x <= xRange.y; x++)
                    {
                        int xDistToEdge = math.min(x - minCoords.x, maxCoords.x - x);

                        if (xDistToEdge < 0)
                        {
                            continue;
                        }

                        int3 coords = new int3(x, y, 0);
                        int3 coordDistToCenter = math.abs(coords - sourceCoord);
                        int maxCoordsDist = math.max(coordDistToCenter.x,
                            math.max(coordDistToCenter.y, coordDistToCenter.z));

                        if (maxCoordsDist != l)
                        {
                            x = xRange.y - 1;
                            continue;
                        }

                        int cellIndex = UniformOriginGrid.GetCellIndexFromCoords(in grid, coords);
                        SpatialDatabaseCell cell = cellsBuffer[cellIndex];
                        collector.OnVisitCell(in cell, in elementsBuffer,
                            out bool shouldEarlyExit);
                        if (shouldEarlyExit)
                        {
                            return;
                        }
                    }
                }
            }
        }
    }
}
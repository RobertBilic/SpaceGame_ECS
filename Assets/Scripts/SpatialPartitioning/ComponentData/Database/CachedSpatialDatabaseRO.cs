using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

public unsafe struct CachedSpatialDatabaseRO
{
    public Entity SpatialDatabaseEntity;
    [ReadOnly]
    public ComponentLookup<SpatialDatabase> SpatialDatabaseLookup;
    [ReadOnly]
    public BufferLookup<SpatialDatabaseCell> CellsBufferLookup;
    [ReadOnly]
    public BufferLookup<SpatialDatabaseElement> ElementsBufferLookup;

    public bool _IsInitialized;
    public SpatialDatabase _SpatialDatabase;
    public UnsafeList<SpatialDatabaseCell> _SpatialDatabaseCells;
    public UnsafeList<SpatialDatabaseElement> _SpatialDatabaseElements;
    public int Team;

    public void CacheData()
    {
        if (!_IsInitialized)
        {
            _SpatialDatabase = SpatialDatabaseLookup[SpatialDatabaseEntity];
            DynamicBuffer<SpatialDatabaseCell> cellsBuffer = CellsBufferLookup[SpatialDatabaseEntity];
            DynamicBuffer<SpatialDatabaseElement> elementsBuffer = ElementsBufferLookup[SpatialDatabaseEntity];
            _SpatialDatabaseCells = new UnsafeList<SpatialDatabaseCell>((SpatialDatabaseCell*)cellsBuffer.GetUnsafeReadOnlyPtr(), cellsBuffer.Length);
            _SpatialDatabaseElements = new UnsafeList<SpatialDatabaseElement>((SpatialDatabaseElement*)elementsBuffer.GetUnsafeReadOnlyPtr(), elementsBuffer.Length);
            _IsInitialized = true;
        }
    }

    public void Dispose()
    {
        _SpatialDatabaseCells.Dispose();
        _SpatialDatabaseElements.Dispose();
        _IsInitialized = false;
    }
}
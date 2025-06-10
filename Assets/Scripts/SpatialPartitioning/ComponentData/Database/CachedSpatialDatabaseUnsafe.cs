using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

public unsafe struct CachedSpatialDatabaseUnsafe
{
    public Entity SpatialDatabaseEntity;
    [NativeDisableParallelForRestriction]
    [NativeDisableContainerSafetyRestriction]
    public ComponentLookup<SpatialDatabase> SpatialDatabaseLookup;
    [NativeDisableParallelForRestriction]
    [NativeDisableContainerSafetyRestriction]
    public BufferLookup<SpatialDatabaseCell> CellsBufferLookup;
    [NativeDisableParallelForRestriction]
    [NativeDisableContainerSafetyRestriction]
    public BufferLookup<SpatialDatabaseElement> ElementsBufferLookup;

    public bool _IsInitialized;
    public int Team;
    public SpatialDatabase _SpatialDatabase;
    public UnsafeList<SpatialDatabaseCell> _SpatialDatabaseCells;
    public UnsafeList<SpatialDatabaseElement> _SpatialDatabaseElements;

    public void CacheData()
    {
        if (!_IsInitialized)
        {
            _SpatialDatabase = SpatialDatabaseLookup[SpatialDatabaseEntity];
            DynamicBuffer<SpatialDatabaseCell> cellsBuffer = CellsBufferLookup[SpatialDatabaseEntity];
            DynamicBuffer<SpatialDatabaseElement> elementsBuffer = ElementsBufferLookup[SpatialDatabaseEntity];
            _SpatialDatabaseCells = new UnsafeList<SpatialDatabaseCell>((SpatialDatabaseCell*)cellsBuffer.GetUnsafePtr(), cellsBuffer.Length);
            _SpatialDatabaseElements = new UnsafeList<SpatialDatabaseElement>((SpatialDatabaseElement*)elementsBuffer.GetUnsafePtr(), elementsBuffer.Length);
            _IsInitialized = true;
        }
    }
}
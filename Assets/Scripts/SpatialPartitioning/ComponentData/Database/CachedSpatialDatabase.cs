using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

public unsafe struct CachedSpatialDatabase
{
    public Entity SpatialDatabaseEntity;
    public ComponentLookup<SpatialDatabase> SpatialDatabaseLookup;
    public BufferLookup<SpatialDatabaseCell> CellsBufferLookup;
    public BufferLookup<SpatialDatabaseElement> ElementsBufferLookup;

    public bool _IsInitialized;
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
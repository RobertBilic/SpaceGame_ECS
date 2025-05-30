
using Unity.Entities;

[InternalBufferCapacity(32)]
public struct SpatialDatabaseCellIndex : IBufferElementData
{
    public int CellIndex;
}
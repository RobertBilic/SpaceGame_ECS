using Unity.Entities;

[InternalBufferCapacity(0)]
public struct SpatialDatabaseCell : IBufferElementData
{
    public int StartIndex;
    public int ElementsCount;
    public int ElementsCapacity;
    public int ExcessElementsCount;
}
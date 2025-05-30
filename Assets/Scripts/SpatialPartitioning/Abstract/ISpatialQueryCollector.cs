using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;

public interface ISpatialQueryCollector
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void OnVisitCell(in SpatialDatabaseCell cell, in UnsafeList<SpatialDatabaseElement> elements, out bool shouldEarlyExit);
}
using Unity.Collections;
using Unity.Entities;

public struct BulletId : IComponentData
{
    public FixedString32Bytes Value;
}

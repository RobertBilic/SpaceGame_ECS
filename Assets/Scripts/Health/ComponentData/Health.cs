using Unity.Entities;

public struct Health : IComponentData
{
    public float Max;
    public float Current;
}

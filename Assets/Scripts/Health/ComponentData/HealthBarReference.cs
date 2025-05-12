using Unity.Entities;

public struct HealthBarReference : IComponentData
{
    public Entity BackgroundEntity;
    public Entity ProgressEntity;
}

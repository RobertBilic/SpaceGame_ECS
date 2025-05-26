using Unity.Entities;

struct DogfightStateComponent : IComponentData
{
    public DogfightState Value;
}

enum DogfightState
{
    Engage,
    Disengage
}
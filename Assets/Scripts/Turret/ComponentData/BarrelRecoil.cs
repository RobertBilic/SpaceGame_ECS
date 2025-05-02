using Unity.Entities;
using Unity.Mathematics;

public struct BarrelRecoilReference : IComponentData
{
    public Entity Entity;
    public float Duration;
    public float MaxDistance;
    public float3 DefaultPosition;
}

public struct BarrelRecoil : IComponentData
{
    public float CurrentTime;
    public float Duration;
    public float MaxDistance;
    public float3 DefaultPosition;
}

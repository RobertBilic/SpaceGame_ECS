using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

public struct OnHitEffectPrefab : IComponentData
{
    public Entity Value;
    public float Lifetime;
}

public struct ImpactParticle : IComponentData
{
    public float Lifetime;
    public float Age;
}

public struct ImpactEffectPoolRequest : IBufferElementData
{
    public Entity Entity;
    public FixedString32Bytes Id;
}

public struct ImpactSpawnRequest : IBufferElementData
{
    public FixedString32Bytes PrefabId;
    public float3 Position;
    public float3 Normal;
    public int Count;
    public float Scale;
}

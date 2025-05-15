using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

public struct OnHitEffectPrefab : IComponentData
{
    public Entity Value;
}

public struct ImpactParticle : IComponentData
{
    public float Lifetime;
    public float Age;
    public float3 Velocity;
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

[MaterialProperty("_Color")]
public struct MaterialProperty__Color : IComponentData
{
    public float4 Value;
}

[MaterialProperty("_Fade")]
public struct MaterialProperty__Fade : IComponentData
{
    public float Value;
}

[MaterialProperty("_DrawOrder")]
public struct MaterialProperty__DrawOrder : IComponentData
{
    public int Value;
}
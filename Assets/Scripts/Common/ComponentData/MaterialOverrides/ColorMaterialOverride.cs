using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

[MaterialProperty("_Color")]
public struct ColorMaterialOverride : IComponentData
{
    public float4 Value;
}

[MaterialProperty("_BackgroundColor")]
public struct BackgroundColorMaterialOverride : IComponentData
{
    public float4 Value;
}

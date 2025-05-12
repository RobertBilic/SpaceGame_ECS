using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

[MaterialProperty("_Color")]
public struct ColorMaterialOverride : IComponentData
{
    public float4 Value;
}

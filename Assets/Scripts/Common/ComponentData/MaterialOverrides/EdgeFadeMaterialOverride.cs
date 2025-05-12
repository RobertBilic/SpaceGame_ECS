using Unity.Entities;
using Unity.Rendering;

[MaterialProperty("_EdgeFade")]
public struct EdgeFadeMaterialOverride : IComponentData
{
    public float Value;
}

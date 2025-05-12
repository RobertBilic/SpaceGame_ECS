using Unity.Entities;
using Unity.Rendering;

[MaterialProperty("_Fill")]
public struct FillMaterialOverrideComponent : IComponentData
{
    public float Value;
}

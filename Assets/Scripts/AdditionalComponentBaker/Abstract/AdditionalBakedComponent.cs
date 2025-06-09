using Unity.Entities;
using UnityEngine;

/// <summary>
/// Helper class to serialize/bake additional component types 
/// </summary>
public abstract class AdditionalBakedComponent<T> : AdditionalBakedComponentBase 
    where T: unmanaged, IComponentData
{
    protected abstract T GetComponentData<TAuthoring>(Baker<TAuthoring> baker) where TAuthoring : Component;

    public override ComponentType GetComponentType() => ComponentType.ReadOnly<T>();

    public override void AddComponent<TAuthoring>(Baker<TAuthoring> baker, Entity entity) 
    {
        baker.AddComponent<T>(entity, GetComponentData(baker));
    }
}

using Unity.Entities;
using UnityEngine;

/// <summary>
/// Helper class to serialize/bake additional component types 
/// </summary>
public abstract class AdditionalBakedComponent<T> : AdditionalBakedComponentBase 
    where T: unmanaged, IComponentData
{
    protected abstract T GetComponentData();
    public override ComponentType GetComponentType() => ComponentType.ReadOnly<T>();

    private void OnValidate()
    {
        if (GetComponentData().GetType() != GetComponentType().GetManagedType())
        {
            Debug.LogWarning($"{GetType().ToString()} has different return types of GetComponentData() and GetComponentType(), is this intended?");
        }
    }

    public override void AddComponent<TAuthoring>(Baker<TAuthoring> baker, Entity entity) 
    {
        baker.AddComponent<T>(entity, GetComponentData());
    }
}

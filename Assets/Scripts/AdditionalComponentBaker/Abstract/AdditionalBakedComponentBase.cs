using Unity.Entities;
using UnityEngine;

public abstract class AdditionalBakedComponentBase : MonoBehaviour
{
    public abstract ComponentType GetComponentType();
    public abstract void AddComponent<TAuthoring>(Baker<TAuthoring> baker, Entity entity) where TAuthoring : Component;
}

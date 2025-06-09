using Unity.Entities;
using UnityEngine;

public abstract class AdditionalBakedBufferBase : MonoBehaviour
{
    public abstract void AddBuffer<TAuthoring>(Baker<TAuthoring> baker, Entity entity) where TAuthoring : Component;
}

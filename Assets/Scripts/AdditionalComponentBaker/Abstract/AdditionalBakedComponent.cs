using Unity.Entities;
using UnityEngine;

/// <summary>
/// Helper class to serialize/bake additional component types 
/// </summary>
public abstract class AdditionalBakedComponent : MonoBehaviour
{
    public abstract ComponentType GetComponentType();
}

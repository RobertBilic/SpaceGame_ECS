using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

/// <summary>
/// Helper class to serialize/bake additional buffer
/// </summary>
public abstract class AdditionalBakedBuffer<T> : AdditionalBakedBufferBase
    where T : unmanaged, IBufferElementData
{
    protected abstract List<T> GetBufferData<TAuthoring>(Baker<TAuthoring> baker) where TAuthoring : Component;

    public override void AddBuffer<TAuthoring>(Baker<TAuthoring> baker, Entity entity)
    {
        var buffer = baker.AddBuffer<T>(entity);

        foreach (var data in GetBufferData(baker))
        {
            buffer.Add(data);
        }

    }
}

using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public abstract class BakerWithHitboxes<T> : Baker<T> where T: MonoWithHitbox 
{
    public override void Bake(T authoring)
    {
        var entity = GetEntity(GetUsageFlags());
        var buffer = AddBuffer<HitBoxElement>(entity);

        foreach (var hitbox in authoring.Hitboxes)
        {
            buffer.Add(new HitBoxElement
            {
                LocalCenter = hitbox.LocalCenter,
                HalfExtents = hitbox.HalfExtents,
                Rotation = quaternion.Euler(math.radians(hitbox.LocalRotationEuler))
            });
        }

        AddComponent(entity, new BoundingRadius() { Value = authoring.BoundingRadius });
        BakeAdditionalData(entity,authoring);
    }

    protected abstract TransformUsageFlags GetUsageFlags();
    protected abstract void BakeAdditionalData(Entity entity,T authoring);
}

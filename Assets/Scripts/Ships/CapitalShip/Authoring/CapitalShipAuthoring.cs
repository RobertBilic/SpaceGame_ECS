using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

public class CapitalShipAuthoring : MonoWithHitbox
{
    public float MoveSpeed;
    public float RotationSpeed;
}

class CapitalShipBaker : BakerWithHitboxes<CapitalShipAuthoring>
{
    protected override void BakeAdditionalData(Entity entity, CapitalShipAuthoring authoring)
    {
        AddComponent(entity, new MoveSpeed { Value = authoring.MoveSpeed });
        AddComponent(entity, new RotationSpeed { Value = authoring.RotationSpeed });
        AddComponent(entity, new CapitalShipTag());
    }

    protected override TransformUsageFlags GetUsageFlags() => TransformUsageFlags.Dynamic;
}

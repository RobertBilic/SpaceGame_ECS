using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

public class CapitalShipAuthoring : MonoWithHitbox
{
    public float MoveSpeed;
    public float RotationSpeed;

    [Header("Health")]
    public float MaxHealth;
    public GameObject HealthBarBackground;
    public GameObject HealthBarProgress;
}

class CapitalShipBaker : BakerWithHitboxes<CapitalShipAuthoring>
{
    protected override void BakeAdditionalData(Entity entity, CapitalShipAuthoring authoring)
    {
        AddComponent(entity, new MoveSpeed { Value = authoring.MoveSpeed });
        AddComponent(entity, new RotationSpeed { Value = authoring.RotationSpeed });
        AddComponent(entity, new CapitalShipTag());

        if(authoring.HealthBarBackground != null && authoring.HealthBarProgress != null)
        {
            AddComponent(entity, new HealthBarReference()
            {
                BackgroundEntity = GetEntity(authoring.HealthBarBackground, TransformUsageFlags.Dynamic),
                ProgressEntity = GetEntity(authoring.HealthBarProgress, TransformUsageFlags.Dynamic)
            });
        }
    }

    protected override TransformUsageFlags GetUsageFlags() => TransformUsageFlags.Dynamic;
}

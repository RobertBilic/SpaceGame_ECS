using System.Collections.Generic;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

class TestEnemy : MonoWithHitbox
{
    public float Speed;
    public float RotationSpeed;
    public float ApproachDistance;

    public float MaxShipBankingAngle;
    public float BankingSmoothSpeed;

    public List<ForwardWeaponAuthoring> Weapons;

    [Header("Health")]
    public GameObject HealthBarBackground;
    public GameObject HealthBarProgress;
}

class TestEnemyBaker : BakerWithHitboxes<TestEnemy>
{
    protected override void BakeAdditionalData(Entity entity, TestEnemy authoring)
    {
        AddComponent(entity, new Health() { Current = 100, Max = 100 });
        AddComponent(entity, new IsAlive() { });
        AddComponent(entity, new TeamTag() { Team = 2 });
        AddComponent(entity, new Team2Tag());
        AddComponent(entity, new MoveSpeed() { Value = authoring.Speed });
        AddComponent(entity, new RotationSpeed() { Value = authoring.RotationSpeed });
        AddComponent(entity, new ApproachDistance() { Value = authoring.ApproachDistance });
        AddComponent(entity, new CurrentRotation() { Value = 0.0f });
        AddComponent(entity, new ShipMovementBehaviourState() { Value = ShipMovementBehaviour.MoveToTarget });
        AddComponent(entity, new ShipBankingData()
        {
            CurrentBankAngle = 0,
            MaxBankAngle = authoring.MaxShipBankingAngle,
            SmoothSpeed = authoring.BankingSmoothSpeed
        });
        AddComponent<TargetableTag>(entity);
        AddBuffer<SpatialDatabaseCellIndex>(entity);
        AddBuffer<DamageHealthRequestBuffer>(entity);
        AddComponent(entity, new Target());
        var weaponBuffer = AddBuffer<ForwardWeaponElement>(entity);

        foreach (var weapon in authoring.Weapons)
            weaponBuffer.Add(new ForwardWeaponElement() { Ref = GetEntity(weapon, TransformUsageFlags.Dynamic) });

        AddComponent(entity, new HealthBarReference()
        {
            BackgroundEntity = GetEntity(authoring.HealthBarBackground, TransformUsageFlags.Dynamic),
            ProgressEntity = GetEntity(authoring.HealthBarProgress, TransformUsageFlags.Dynamic)
        });
    }

    protected override TransformUsageFlags GetUsageFlags() => TransformUsageFlags.Dynamic;
}

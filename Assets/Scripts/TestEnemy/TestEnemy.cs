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
}

class TestEnemyBaker : BakerWithHitboxes<TestEnemy>
{
    protected override void BakeAdditionalData(Entity entity, TestEnemy authoring)
    {
        AddComponent(entity, new Health() { Value = 100 });
        AddComponent(entity, new IsAlive() { Value = true });
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
        AddComponent(entity, new TeamTag() { Team = 2 });
        AddComponent(entity, new SpatialDatabaseCellIndex());
    }

    protected override TransformUsageFlags GetUsageFlags() => TransformUsageFlags.Dynamic;
}

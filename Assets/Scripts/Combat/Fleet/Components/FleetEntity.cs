using Unity.Entities;

public static class FleetConstants
{
    public const float MaximumCohesion = 100.0f;
    public const float AcceptableDistanceForMaximumCohesion = 1.0f;
    public const float SpeedMultiplierOnZeroCohesion = 0.2f;
    public const float DistanceForMaximumCohesionLoss = 5.0f;
}

public struct FleetEntity : IComponentData
{
    public Entity Leader;
    public float MaxSpeed;
    public float Cohesion;
    public float CohesionSpeedMultiplier;
}

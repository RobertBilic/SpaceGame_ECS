using Unity.Entities;

public struct ThrustSettings : IComponentData
{
    public float MaxSpeed;
    public float Acceleration;
    public float Decceleration;
    public float SpeedRotationPenalty;
}

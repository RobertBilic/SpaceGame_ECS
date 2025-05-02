using Unity.Entities;

public struct ShipMovementBehaviourState : IComponentData
{
    public ShipMovementBehaviour Value;
}

public enum ShipMovementBehaviour
{
    MoveToTarget,
    Disengage,
    Reengage
}
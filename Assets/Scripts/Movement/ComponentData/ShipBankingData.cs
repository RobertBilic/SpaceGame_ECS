using Unity.Entities;

public struct ShipBankingData : IComponentData
{
    public float MaxBankAngle;   
    public float SmoothSpeed;      
    public float CurrentBankAngle;  
}
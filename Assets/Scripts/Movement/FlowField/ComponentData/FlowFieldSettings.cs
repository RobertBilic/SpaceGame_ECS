using Unity.Entities;
using Unity.Mathematics;

public struct FlowFieldSettings : IComponentData
{
    public float2 WorldSize;    
    public float CellSize;      
}

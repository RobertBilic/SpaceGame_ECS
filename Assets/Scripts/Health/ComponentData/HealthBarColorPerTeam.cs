using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct HealthBarColorPerTeam : IBufferElementData
{
    public float4 Color;
    public int Team;
}

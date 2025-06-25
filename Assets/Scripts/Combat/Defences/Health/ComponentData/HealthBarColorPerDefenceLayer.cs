using SpaceGame.Combat.Defences;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public struct HealthBarColorPerDefenceLayer : IBufferElementData
{
    public float4 Color;
    public DefenceLayerType Layer;
    public int Team;
}

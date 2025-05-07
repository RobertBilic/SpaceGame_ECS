using Unity.Entities;
using Unity.Mathematics;

namespace SpaceGame.Movement.Flowfield.Components
{
    public struct FlowFieldSettings : IComponentData
    {
        public float2 WorldSize;
        public float CellSize;
    }
}
using Unity.Entities;
using Unity.Mathematics;

namespace SpaceGame.Movement.Flowfield.Components
{
    public struct FlowFieldCell : IBufferElementData
    {
        public float2 Direction;
    }
}
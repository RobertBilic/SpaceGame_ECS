using Unity.Entities;

namespace SpaceGame.Combat.Defences
{
    public struct DefenceLayer : IBufferElementData
    {
        public DefenceLayerType Type;
        public float Value;
        public float Max;
    }
}

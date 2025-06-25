using Unity.Entities;

namespace SpaceGame.Combat.Defences
{
    public struct ActiveDefenceLayer : IComponentData
    {
        public DefenceLayerType Value;
    }
}

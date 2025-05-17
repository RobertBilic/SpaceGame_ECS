using Unity.Entities;

namespace SpaceGame.Combat.Components
{
    public struct LastFireTime : IComponentData
    {
        public double Value;
    }
}
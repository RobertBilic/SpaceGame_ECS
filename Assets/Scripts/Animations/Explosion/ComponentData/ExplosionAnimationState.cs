using Unity.Entities;

namespace SpaceGame.Animations.Components
{
    public struct ExplosionAnimationState : IComponentData
    {
        public float TimeSinceLastFrame;
        public float TimeUntilNextFrame;
        public int CurrentFrame;
    }
}
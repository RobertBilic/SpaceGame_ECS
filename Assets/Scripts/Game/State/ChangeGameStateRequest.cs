using Unity.Entities;

namespace SpaceGame.Game.State.Component
{
    public struct ChangeGameStateRequest : IComponentData
    {
        public GameState Value;
    }
}
using Unity.Entities;

namespace SpaceGame.Game.State.Component
{
    public struct GameStateComponent : IComponentData
    {
        public GameState Value;
    }

    public enum GameState : byte
    {
        None = 0,
        MainMenu = 1,
        FleetManagement = 2,
        LevelSelection = 4,
        Combat = 8,
    }
}
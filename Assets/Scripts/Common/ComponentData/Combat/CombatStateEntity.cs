using Unity.Entities;

namespace SpaceGame.Combat.Components
{
    /// <summary>
    /// Entity created during the combat state, needs to be disposed after state changes
    /// </summary>
    public struct CombatStateEntity : IComponentData
    {
    }
}

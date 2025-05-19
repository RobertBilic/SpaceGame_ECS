using SpaceGame.Game.Initialization.Components;
using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class CombatSystemGroup : ComponentSystemGroup {
    protected override void OnCreate()
    {
        base.OnCreate();
        RequireForUpdate<GameInitializedTag>();
    }
}

[UpdateInGroup(typeof(CombatSystemGroup), OrderFirst = true)]
public partial class CombatInitializationGroup : ComponentSystemGroup { }

[UpdateInGroup(typeof(CombatSystemGroup))]
[UpdateAfter(typeof(CombatInitializationGroup))]
public partial class CombatMovementGroup : ComponentSystemGroup { }

[UpdateInGroup(typeof(CombatSystemGroup))]
[UpdateAfter(typeof(CombatMovementGroup))]
public partial class CombatTargetingGroup : ComponentSystemGroup{ }

[UpdateInGroup(typeof(CombatSystemGroup))]
[UpdateAfter(typeof(CombatTargetingGroup))]
public partial class CombatFiringGroup : ComponentSystemGroup { }

[UpdateInGroup(typeof(CombatSystemGroup))]
[UpdateAfter(typeof(CombatFiringGroup))]
public partial class CombatCollisionGroup : ComponentSystemGroup { }

[UpdateInGroup(typeof(CombatSystemGroup))]
[UpdateAfter(typeof(CombatCollisionGroup))]
public partial class CombatLateUpdateGroup : ComponentSystemGroup { }

[UpdateInGroup(typeof(CombatSystemGroup))]
[UpdateAfter(typeof(CombatLateUpdateGroup))]
public partial class CombatAnimationGroup : ComponentSystemGroup { }
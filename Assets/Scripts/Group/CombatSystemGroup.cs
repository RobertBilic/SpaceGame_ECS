using Unity.Entities;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class CombatSystemGroup : ComponentSystemGroup { }

[UpdateInGroup(typeof(CombatSystemGroup), OrderFirst = true)]
public partial class CombatMovementGroup : ComponentSystemGroup { }

[UpdateInGroup(typeof(CombatSystemGroup))]
[UpdateAfter(typeof(CombatMovementGroup))]
public partial class CombatTargetingGroup : ComponentSystemGroup { }

[UpdateInGroup(typeof(CombatSystemGroup))]
[UpdateAfter(typeof(CombatTargetingGroup))]
public partial class CombatFiringGroup : ComponentSystemGroup { }

[UpdateInGroup(typeof(CombatSystemGroup))]
[UpdateAfter(typeof(CombatFiringGroup))]
public partial class CombatCollisionGroup : ComponentSystemGroup { }

[UpdateInGroup(typeof(CombatSystemGroup))]
[UpdateAfter(typeof(CombatCollisionGroup))]
public partial class CombatAnimationGroup : ComponentSystemGroup { }
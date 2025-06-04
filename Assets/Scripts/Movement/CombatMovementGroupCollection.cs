using Unity.Entities;



[UpdateInGroup(typeof(CombatMovementGroup), OrderFirst = true)]
public partial class CombatMovementCalculationGroup : ComponentSystemGroup { }

[UpdateInGroup(typeof(CombatMovementGroup))]
[UpdateAfter(typeof(CombatMovementCalculationGroup))]
public partial class CombatMovementExecutionGroup : ComponentSystemGroup { }


[UpdateInGroup(typeof(CombatMovementGroup))]
[UpdateAfter(typeof(CombatMovementExecutionGroup))]
public partial class CombatFleetMovementCalculationGroup : ComponentSystemGroup { }

[UpdateInGroup(typeof(CombatMovementGroup))]
[UpdateAfter(typeof(CombatFleetMovementCalculationGroup))]
public partial class CombatFleetMovementExecutionGroup : ComponentSystemGroup { }
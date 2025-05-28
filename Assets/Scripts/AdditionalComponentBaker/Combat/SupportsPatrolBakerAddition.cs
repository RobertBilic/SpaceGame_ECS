using SpaceGame.Combat.Patrol.Components;
using UnityEngine;

public class SupportsPatrolBakerAddition : AdditionalBakedComponent<SupportsPatrolTag>
{
    protected override SupportsPatrolTag GetComponentData() => new SupportsPatrolTag();
    
}

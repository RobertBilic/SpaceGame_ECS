using Unity.Entities;

public class CombatEntityBakerAddition : AdditionalBakedComponent
{
    public override ComponentType GetComponentType() => ComponentType.ReadOnly<CombatEntity>();
}

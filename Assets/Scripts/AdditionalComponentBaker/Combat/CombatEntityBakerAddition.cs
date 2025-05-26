using Unity.Entities;
using UnityEngine;

public class CombatEntityBakerAddition : AdditionalBakedComponent<CombatEntity>
{
    protected override CombatEntity GetComponentData() => new CombatEntity();
}

using Unity.Entities;
using UnityEngine;

public class CombatEntityBakerAddition : AdditionalBakedComponent<CombatEntity>
{
    protected override CombatEntity GetComponentData<TAuthoring>(Baker<TAuthoring> baker) => new CombatEntity();
}

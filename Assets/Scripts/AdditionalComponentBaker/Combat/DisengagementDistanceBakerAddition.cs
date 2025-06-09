using UnityEngine;

public class DisengagementDistanceBakerAddition : AdditionalBakedComponent<DisengageDistance>
{
    [SerializeField]
    private float distance;

    protected override DisengageDistance GetComponentData<TAuthoring>(Unity.Entities.Baker<TAuthoring> baker)
    {
        return new DisengageDistance()
        {
            Value = distance
        };
    }
}

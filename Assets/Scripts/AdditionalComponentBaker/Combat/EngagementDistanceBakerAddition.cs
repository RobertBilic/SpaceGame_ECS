using UnityEngine;

public class EngagementDistanceBakerAddition : AdditionalBakedComponent<EngageDistance>
{
    [SerializeField]
    private float distance;

    protected override EngageDistance GetComponentData<TAuthoring>(Unity.Entities.Baker<TAuthoring> baker)
    {
        return new EngageDistance()
        {
            Value = distance
        };
    }
}

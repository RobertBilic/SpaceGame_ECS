using UnityEngine;

public class EngagementDistanceBakerAddition : AdditionalBakedComponent<EngageDistance>
{
    [SerializeField]
    private float distance;

    protected override EngageDistance GetComponentData()
    {
        return new EngageDistance()
        {
            Value = distance
        };
    }
}

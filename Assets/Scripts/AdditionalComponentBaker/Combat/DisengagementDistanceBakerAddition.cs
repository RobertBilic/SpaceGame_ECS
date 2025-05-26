using UnityEngine;

public class DisengagementDistanceBakerAddition : AdditionalBakedComponent<DisengageDistance>
{
    [SerializeField]
    private float distance;
    protected override DisengageDistance GetComponentData()
    {
        return new DisengageDistance()
        {
            Value = distance
        };
    }
}

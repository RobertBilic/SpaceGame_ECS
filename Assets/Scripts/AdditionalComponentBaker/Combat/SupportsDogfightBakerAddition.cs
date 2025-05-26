using Unity.Entities;
using UnityEngine;

public class SupportsDogfightBakerAddition : AdditionalBakedComponent<SupportsDogfightTag>
{
    protected override SupportsDogfightTag GetComponentData() => new SupportsDogfightTag();
}

using SpaceGame.Combat.Components;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

class BulletAuthoring : MonoBehaviour
{
    public GameObject OnHitPrefab;

    public float Speed;
    public float Scale;
}

class BulletAuthoringBaker : Baker<BulletAuthoring>
{
    public override void Bake(BulletAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);

        AddComponent(entity, new MoveSpeed() { Value = authoring.Speed });
        AddComponent(entity, new Radius() { Value = authoring.Scale });
        AddComponent(entity, new BulletTag());
        AddComponent(entity, new Lifetime { Value = 0.0f });
        AddComponent(entity, new Heading() {  });
        AddComponent(entity, new PreviousPosition() { });
        AddComponent(entity, new Damage() { });
        AddComponent(entity, new TeamTag() );
        AddComponent(entity, new OnHitEffectPrefab() 
        { 
            Value = GetEntity(authoring.OnHitPrefab, TransformUsageFlags.Dynamic)
        });
        AddComponent(entity, new BulletId() { });
        AddComponent(entity, new Disabled());
    }
}

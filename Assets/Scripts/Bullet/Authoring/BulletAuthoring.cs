using SpaceGame.Combat.Components;
using Unity.Entities;
using UnityEngine;

class BulletAuthoring : MonoBehaviour
{
    public float Speed;
}

class BulletAuthoringBaker : Baker<BulletAuthoring>
{
    public override void Bake(BulletAuthoring authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);

        AddComponent(entity, new MoveSpeed() { Value = authoring.Speed });
    }
}

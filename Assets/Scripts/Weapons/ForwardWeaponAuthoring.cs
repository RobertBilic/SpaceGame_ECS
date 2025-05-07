using SpaceGame.Animations.Components;
using SpaceGame.Combat.Components;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

class ForwardWeaponAuthoring : MonoBehaviour
{
    public Transform Parent;
    public GameObject RotationBase;
    public int TeamTag;

    [Header("Rotation")]
    public float RotationSpeed;
    public float MaximumRotationAngle;
    public float RotationPadding;

    [Header("Combat Stats")]
    public float Range;
    public float Damage;
    public float FiringRate;

    
    [Header("Recoil")]
    public GameObject RecoilTarget;
    public float RecoilDuration;
    public float MaxRecoilDistance;

    [Header("Bullet")]
    public string BulletId;
    public List<Vector3> BulletSpawnPositionOffset;

    class ForwardWeaponAuthroingBaker : Baker<ForwardWeaponAuthoring>
    {
        public override void Bake(ForwardWeaponAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new ForwardWeapon()
            {
                Damage = authoring.Damage,
                FiringRate = authoring.FiringRate,
                LastFireTime = 0,
                Range = authoring.Range,
                RotationBase = GetEntity(authoring.RotationBase, TransformUsageFlags.Dynamic),
                MaxRotationAngleDeg = authoring.MaximumRotationAngle,
                CurrentRotationOffsetDeg = 0.0f,
                MaxRotationPadding = authoring.RotationPadding,
                RotationSpeed = authoring.RotationSpeed,
                BulletId = authoring.BulletId
            }); 

            AddComponent(entity, new ForwardWeaponTag());
            AddComponent(entity, new TeamTag() { Team = authoring.TeamTag });

            if (authoring.RecoilTarget != null)
            {
                AddComponent(entity, new BarrelRecoilReference()
                {
                    Duration = authoring.RecoilDuration,
                    MaxDistance = authoring.MaxRecoilDistance,
                    Entity = GetEntity(authoring.RecoilTarget, TransformUsageFlags.Dynamic),
                    DefaultPosition = authoring.RecoilTarget.transform.localPosition
                });
            }

            var spawnOffsetBuffer = AddBuffer<ProjectileSpawnOffset>(entity);

            foreach (var offset in authoring.BulletSpawnPositionOffset)
            {
                spawnOffsetBuffer.Add(new ProjectileSpawnOffset() { Value = offset });
            }

            if(authoring.Parent != null)
            {
                AddComponent(entity, new Parent() { Value = GetEntity(authoring.Parent, TransformUsageFlags.None) });
            }
        }
    }

}

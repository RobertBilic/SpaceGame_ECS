using SpaceGame.Combat.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(CombatSystemGroup))]
[BurstCompile]
public partial struct ForwardWeaponFiringSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Target>();
        state.RequireForUpdate<ForwardWeaponElement>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.TryGetSingleton<BulletPrefabLookupSingleton>(out var blobSingleton))
            return;

        ref var lookup = ref blobSingleton.Lookup.Value;

        var elapsedTime = (float)SystemAPI.Time.ElapsedTime;
        var deltaTime = SystemAPI.Time.DeltaTime;
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var weaponLookup = state.GetComponentLookup<ForwardWeapon>(false);
        var localTransformLookup = state.GetComponentLookup<LocalTransform>(false);
        var localToWorldLookup = state.GetComponentLookup<LocalToWorld>(true);
        var offsetBufferLookup = state.GetBufferLookup<ProjectileSpawnOffset>(true);

        foreach (var (weaponBuffer, shipLtw, target, team, shipEntity) in SystemAPI.Query<
                     DynamicBuffer<ForwardWeaponElement>,
                     RefRO<LocalToWorld>,
                     RefRO<Target>,
                     RefRO<TeamTag>>().WithEntityAccess())
        {
            if (target.ValueRO.Value == Entity.Null || !SystemAPI.Exists(target.ValueRO.Value))
                continue;

            float3 targetPos = SystemAPI.GetComponent<LocalToWorld>(target.ValueRO.Value).Position;

            for (int i = 0; i < weaponBuffer.Length; i++)
            {
                Entity weaponEntity = weaponBuffer[i].Ref;

                if (!weaponLookup.HasComponent(weaponEntity))
                    continue;

                var weapon = weaponLookup[weaponEntity];

                var prefabData = lookup.GetPrefab(weapon.BulletId);

                if (prefabData.Entity == Entity.Null || !state.EntityManager.Exists(prefabData.Entity))
                    continue;

                if (!localTransformLookup.HasComponent(weapon.RotationBase) ||
                    !localToWorldLookup.HasComponent(weapon.RotationBase))
                    continue;

                var baseLtw = localToWorldLookup[weapon.RotationBase];
                float3 weaponPos = baseLtw.Position;
                float3 baseForward = math.normalize(baseLtw.Forward); 

                float3 toTarget = math.normalize(targetPos - weaponPos);

                float3 flatForward = math.normalize(new float3(baseForward.x, baseForward.y, 0));
                float3 flatToTarget = math.normalize(new float3(toTarget.x, toTarget.y, 0));

                float angleToTargetDeg = math.degrees(math.atan2(flatToTarget.y, flatToTarget.x) -
                                      math.atan2(flatForward.y, flatForward.x));

                angleToTargetDeg = NormalizeAngleDegrees(angleToTargetDeg);

                float maxStep = weapon.RotationSpeed * deltaTime;
                float clampedStep = math.clamp(angleToTargetDeg, -maxStep, maxStep);

                weapon.CurrentRotationOffsetDeg = math.clamp(
                    weapon.CurrentRotationOffsetDeg + clampedStep,
                    -weapon.MaxRotationAngleDeg,
                    weapon.MaxRotationAngleDeg);

                if (localTransformLookup.HasComponent(weapon.RotationBase))
                {
                    var localTransform = localTransformLookup[weapon.RotationBase];
                    quaternion rotation = quaternion.RotateY(math.radians(weapon.CurrentRotationOffsetDeg));
                    localTransform.Rotation = rotation;
                    localTransformLookup[weapon.RotationBase] = localTransform;
                }

                float3 finalForward = math.mul(
                    quaternion.RotateY(math.radians(weapon.CurrentRotationOffsetDeg)), flatForward);

                float alignment = math.dot(finalForward, flatToTarget);
                float maxDot = math.cos(math.radians(weapon.MaxRotationAngleDeg));

                if (alignment < maxDot)
                    continue;

                if ((elapsedTime - weapon.LastFireTime) < (1f / weapon.FiringRate))
                    continue;

                weapon.LastFireTime = elapsedTime;
                weaponLookup[weaponEntity] = weapon;

                if (!offsetBufferLookup.HasBuffer(weaponEntity))
                    continue;

                var spawnOffsets = offsetBufferLookup[weaponEntity];
                foreach (var offset in spawnOffsets)
                {
                    float3 spawnPosition = math.transform(baseLtw.Value, offset.Value);
                    spawnPosition.z = 0.0f;

                    var bulletEntity = ecb.Instantiate(prefabData.Entity);

                    ecb.SetComponent(bulletEntity, new LocalTransform
                    {
                        Position = spawnPosition,
                        Rotation = quaternion.identity,
                        Scale = prefabData.Scale
                    });

                    ecb.AddComponent(bulletEntity, new BulletTag());
                    ecb.AddComponent(bulletEntity, new Lifetime { Value = 3.0f });
                    ecb.AddComponent(bulletEntity, new MoveSpeed() { Value = prefabData.Speed });
                    ecb.AddComponent(bulletEntity, new Heading() { Value = finalForward });
                    ecb.AddComponent(bulletEntity, new Radius() { Value = prefabData.Scale });
                    ecb.AddComponent(bulletEntity, new PreviousPosition() { Value = spawnPosition });
                    ecb.AddComponent(bulletEntity, new Damage() { Value = weapon.Damage });
                    ecb.AddComponent(bulletEntity, new TeamTag() { Team = team.ValueRO.Team });
                    ecb.AddComponent(bulletEntity, new OnHitEffectPrefab() { Value = prefabData.OnHitEntity });
                }
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
    private float NormalizeAngleDegrees(float angle)
    {
        angle %= 360f;
        if (angle > 180f)
            angle -= 360f;
        else if (angle < -180f)
            angle += 360f;
        return angle;
    }
}

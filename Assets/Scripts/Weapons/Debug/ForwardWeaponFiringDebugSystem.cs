using SpaceGame.Combat.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace SpaceGame.Combat.Systems.Debug
{
    [UpdateInGroup(typeof(CombatFiringGroup))]
    [UpdateBefore(typeof(ForwardWeaponFiringSystem))]
    partial struct ForwardWeaponFiringDebugSystem : ISystem
    {
        private ComponentLookup<ForwardWeapon> weaponLookup;
        private ComponentLookup<LocalTransform> localTransformLookup;
        private ComponentLookup<LocalToWorld> localToWorldLookup;
        private BufferLookup<ProjectileSpawnOffset> offsetBufferLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Target>();
            state.RequireForUpdate<ForwardWeaponElement>();
            state.RequireForUpdate<EnableForwardWeaponDebug>();

            weaponLookup = state.GetComponentLookup<ForwardWeapon>(true);
            localTransformLookup = state.GetComponentLookup<LocalTransform>(true);
            localToWorldLookup = state.GetComponentLookup<LocalToWorld>(true);
            offsetBufferLookup = state.GetBufferLookup<ProjectileSpawnOffset>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<GlobalTimeComponent>(out var timeComp))
                return;

            weaponLookup.Update(ref state);
            localTransformLookup.Update(ref state);
            localToWorldLookup.Update(ref state);
            offsetBufferLookup.Update(ref state);

            var elapsedTime = timeComp.ElapsedTimeScaled;
            var deltaTime = timeComp.DeltaTimeScaled; 

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

                    if (!localTransformLookup.HasComponent(weapon.RotationBase) ||
                        !localToWorldLookup.HasComponent(weapon.RotationBase))
                        continue;

                    var baseLtw = localToWorldLookup[weapon.RotationBase];
                    float3 weaponPos = baseLtw.Position;

                    float3 toTargetRaw = targetPos - weaponPos;
                    if (math.lengthsq(toTargetRaw) < 0.0001f)
                        continue;

                    float3 toTarget = math.normalize(toTargetRaw);

                    UnityEngine.Debug.DrawLine(weaponPos, targetPos, Color.green / 2.0f, 0.0f);
                    float3 baseForward = math.mul(baseLtw.Rotation, new float3(1, 0, 0));

                    if (!math.isfinite(baseForward.x) || !math.isfinite(baseForward.y))
                        continue;

                    float3 flatForward = new float3(baseForward.x, baseForward.y, 0);
                    if (math.lengthsq(flatForward) < 0.0001f)
                        continue;

                    float3 flatToTarget = new float3(toTarget.x, toTarget.y, 0);
                    if (math.lengthsq(flatToTarget) < 0.0001f)
                        continue;

                    flatForward = math.normalize(flatForward);
                    flatToTarget = math.normalize(flatToTarget); 
                    
                    UnityEngine.Debug.DrawRay(weaponPos, flatForward * 2f, Color.blue/2.0f);   
                    UnityEngine.Debug.DrawRay(weaponPos, flatToTarget * 2f, Color.red/2.0f);  

                    float angleToTargetDeg = math.degrees(math.atan2(flatToTarget.y, flatToTarget.x) -
                                          math.atan2(flatForward.y, flatForward.x));

                    angleToTargetDeg = NormalizeAngleDegrees(angleToTargetDeg);

                    UnityEngine.Debug.Log($"Entity {shipEntity.Index} with angle with weapon index {i} angle: {angleToTargetDeg} degrees");
                        
                    float maxStep = weapon.RotationSpeed * deltaTime;
                    float clampedStep = math.clamp(angleToTargetDeg, -maxStep, maxStep);

                    weapon.CurrentRotationOffsetDeg = math.clamp(
                        weapon.CurrentRotationOffsetDeg + clampedStep,
                        -weapon.MaxRotationAngleDeg,
                        weapon.MaxRotationAngleDeg);

                    if (localTransformLookup.HasComponent(weapon.RotationBase))
                    {
                        var localTransform = localTransformLookup[weapon.RotationBase];
                        quaternion rotation = quaternion.RotateZ(math.radians(weapon.CurrentRotationOffsetDeg));
                        localTransform.Rotation = rotation;
                    }

                    float3 finalForward = math.mul(quaternion.RotateZ(math.radians(weapon.CurrentRotationOffsetDeg)), flatForward);
                    finalForward.z = 0.0f;


                    float alignment = math.dot(finalForward, flatToTarget);
                    float maxDot = math.cos(math.radians(weapon.MaxRotationAngleDeg));

                    if (alignment < maxDot)
                        continue;

                    if ((elapsedTime - weapon.LastFireTime) < (1f / weapon.FiringRate))
                        continue;

                    if (!offsetBufferLookup.HasBuffer(weaponEntity))
                        continue;

                    var spawnOffsets = offsetBufferLookup[weaponEntity];
                    foreach (var offset in spawnOffsets)
                    {
                        float3 spawnPosition = math.transform(baseLtw.Value, offset.Value);
                        spawnPosition.z = 0.0f;
                    }
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

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
}
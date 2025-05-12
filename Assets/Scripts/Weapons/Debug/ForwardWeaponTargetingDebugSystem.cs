using SpaceGame.Combat.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace SpaceGame.Combat.Systems.Debug
{
    [UpdateInGroup(typeof(CombatTargetingGroup))]
    [UpdateAfter(typeof(ForwardWeaponTargetingSystem))]
    partial struct ForwardWeaponTargetingDebugSystem : ISystem
    {

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EnableForwardWeaponDebug>();
            state.RequireForUpdate<ForwardWeapon>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach(var (localToWorld, weaponRefs, entity) in SystemAPI.Query<RefRO<LocalToWorld>, DynamicBuffer<ForwardWeaponElement>>().WithEntityAccess())
            {
                var ltw = localToWorld.ValueRO;

                float3 heading = math.mul(ltw.Rotation, new float3(1.0f, 0.0f, 0.0f));
                heading.z = 0.0f;

                float3 position = ltw.Position;

                float maxRange = 0;
                float maxAngle = 0;

                for (int i = 0; i < weaponRefs.Length; i++)
                {
                    Entity weaponEntity = weaponRefs[i].Ref;

                    if (!state.EntityManager.HasComponent<ForwardWeapon>(weaponEntity))
                        continue;

                    var weapon = state.EntityManager.GetComponentData<ForwardWeapon>(weaponEntity);
                    maxRange = math.max(maxRange, weapon.Range);
                    maxAngle = math.max(maxAngle, weapon.MaxRotationAngleDeg + weapon.MaxRotationPadding);
                }

                var hasTarget = false;

                if(state.EntityManager.HasComponent<Target>(entity))
                {
                    var target = state.EntityManager.GetComponentData<Target>(entity);

                    hasTarget = target.Value != Entity.Null && state.EntityManager.Exists(target.Value);
                }

                var color = hasTarget ? Color.green : Color.red;

                UnityEngine.Debug.DrawLine(position, position + heading * maxRange, color,0.0f);
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}
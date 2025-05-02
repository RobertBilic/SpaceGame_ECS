using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using SpaceGame.Debug.Components;
using SpaceGame.Combat.Components;

namespace SpaceGame.Debug.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial struct BulletCollisionDebugSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EnableDebugTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var bulletColor = Color.cyan;
            var hitboxColor = Color.red;

            foreach (var (transform, prevPos) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<PreviousPosition>>().WithAll<BulletTag>())
            {
                UnityEngine.Debug.DrawLine(prevPos.ValueRO.Value, transform.ValueRO.Position, bulletColor);
            }

            foreach (var (shipTransform, hitboxes) in SystemAPI.Query<RefRO<LocalTransform>, DynamicBuffer<HitBoxElement>>())
            {
                float3 shipWorldPos = shipTransform.ValueRO.Position;

                float3 shipHeading = math.mul(shipTransform.ValueRO.Rotation, new float3(1f, 0f, 0f));
                shipHeading.z = 0f;
                shipHeading = math.normalize(shipHeading);

                float angle = math.atan2(shipHeading.y, shipHeading.x);
                quaternion flatRotation = quaternion.RotateZ(angle);

                foreach (var hitbox in hitboxes)
                {
                    DrawHitbox(shipWorldPos, flatRotation, hitbox, hitboxColor);
                }
            }
        }

        private void DrawHitbox(float3 shipPos, quaternion shipRot, HitBoxElement hitbox, Color color)
        {
            float3 centerWorld = shipPos + math.mul(shipRot, hitbox.LocalCenter);
            quaternion rotWorld = math.mul(shipRot, hitbox.Rotation);

            float3 right = math.mul(rotWorld, new float3(1, 0, 0)) * hitbox.HalfExtents.x;
            float3 up = math.mul(rotWorld, new float3(0, 1, 0)) * hitbox.HalfExtents.y;

            UnityEngine.Debug.DrawLine(centerWorld - right - up, centerWorld + right - up, color);
            UnityEngine.Debug.DrawLine(centerWorld + right - up, centerWorld + right + up, color);
            UnityEngine.Debug.DrawLine(centerWorld + right + up, centerWorld - right + up, color);
            UnityEngine.Debug.DrawLine(centerWorld - right + up, centerWorld - right - up, color);
        }
    }
}
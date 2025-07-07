using SpaceGame.Combat.Components;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SpaceGame.Combat.QueryCollectors
{
    [BurstCompile]
    public struct BulletCollisionDetector : ISpatialQueryCollector
    {
        public bool isEnemyHit;
        public Entity HitEntity;

        private int team;

        private float3 bulletStart;
        private float3 bulletEnd;
        private float radius;

        private ComponentLookup<TeamTag> teamLookup;
        private ComponentLookup<LocalToWorld> ltwLookup;
        private ComponentLookup<LocalTransform> ltLookup;
        private ComponentLookup<BoundingRadius> radiusLookup;
        private BufferLookup<HitBoxElement> hitboxLookup;

        public BulletCollisionDetector(ComponentLookup<TeamTag> teamLookup, ComponentLookup<LocalToWorld> ltwLookup,
             ComponentLookup<LocalTransform> ltLookup, ComponentLookup<BoundingRadius> radiusLookup,
             BufferLookup<HitBoxElement> hitboxLookup, int team, float3 bulletStart, float3 bulletEnd, float radius) : this()
        {
            this.teamLookup = teamLookup;
            this.ltwLookup = ltwLookup;
            this.ltLookup = ltLookup;
            this.radiusLookup = radiusLookup;
            this.hitboxLookup = hitboxLookup;

            this.team = team;
            this.bulletEnd = bulletEnd;
            this.bulletStart = bulletStart;
            this.radius = radius;
        }

        [BurstCompile]
        public void OnVisitCell(in SpatialDatabaseCell cell, in UnsafeList<SpatialDatabaseElement> elements, out bool shouldEarlyExit)
        {
            shouldEarlyExit = false;

            for (int i = cell.StartIndex; i < cell.StartIndex + cell.ElementsCount; i++)
            {
                var element = elements[i];

                if (element.Entity == Entity.Null)
                    continue;

                if (teamLookup.HasComponent(element.Entity))
                {
                    var targetTeam = teamLookup[element.Entity];
                    if (targetTeam.Team == team)
                        continue;
                }

                if (!ltwLookup.HasComponent(element.Entity))
                    continue;

                var worldTransform = ltwLookup[element.Entity];
                var boundingRadius = radiusLookup[element.Entity];
                var scaledRadius = boundingRadius.Value;

                var distanceToLineSeg = DistancePointToSegment(worldTransform.Position, bulletStart, bulletEnd);

                if (distanceToLineSeg > scaledRadius + radius)
                    continue;

                float3 heading = math.normalize(bulletEnd - bulletStart);
                float3 right = new float3(-heading.y, heading.x, 0f);
                float3 up = new float3(0f, 1f, 0f);

                float3 rightOffset = right * (radius / 2f);
                float3 upOffset = up * (radius / 2f);

                float3 startR = bulletStart + rightOffset;
                float3 endR = bulletEnd + rightOffset;

                float3 startL = bulletStart - rightOffset;
                float3 endL = bulletEnd - rightOffset;

                float3 startU = bulletStart + upOffset;
                float3 endU = bulletEnd + upOffset;

                float3 startD = bulletStart - upOffset;
                float3 endD = bulletEnd - upOffset;

                var shipTransform = ltLookup[element.Entity];
                var hitboxes = hitboxLookup[element.Entity];

                float3 shipWorldPos = shipTransform.Position;
                quaternion shipWorldRot = shipTransform.Rotation;

                float3 shipForward = math.mul(shipWorldRot, new float3(1f, 0f, 0f));
                shipForward.z = 0f;
                shipForward = math.normalize(shipForward);

                float angle = math.atan2(shipForward.y, shipForward.x);
                quaternion flatRotation = quaternion.RotateZ(angle);

                for (int j = 0; j < hitboxes.Length; j++)
                {
                    var hitbox = hitboxes[j];

                    float3 hitboxWorldCenter = shipWorldPos + math.mul(flatRotation, hitbox.LocalCenter * worldTransform.Value.Scale());
                    quaternion hitboxWorldRotation = math.mul(flatRotation, hitbox.Rotation);

                    float4x4 hitboxWorldToLocal = math.inverse(new float4x4(hitboxWorldRotation, hitboxWorldCenter));

                    float3 halfExtents = hitbox.HalfExtents * worldTransform.Value.Scale();

                    float3 localStartR = math.transform(hitboxWorldToLocal, startR);
                    float3 localEndR = math.transform(hitboxWorldToLocal, endR);

                    float3 localStartL = math.transform(hitboxWorldToLocal, startL);
                    float3 localEndL = math.transform(hitboxWorldToLocal, endL);

                    float3 localStartU = math.transform(hitboxWorldToLocal, startU);
                    float3 localEndU = math.transform(hitboxWorldToLocal, endU);

                    float3 localStartD = math.transform(hitboxWorldToLocal, startD);
                    float3 localEndD = math.transform(hitboxWorldToLocal, endD);

                    if (LineSegmentIntersectsAABB(localStartR, localEndR, halfExtents) ||
                        LineSegmentIntersectsAABB(localStartL, localEndL, halfExtents) ||
                        LineSegmentIntersectsAABB(localStartU, localEndU, halfExtents) ||
                        LineSegmentIntersectsAABB(localStartD, localEndD, halfExtents))
                    {
                        isEnemyHit = true;
                        HitEntity = element.Entity;
                        break;
                    }
                }

                if (isEnemyHit)
                {
                    shouldEarlyExit = true;
                    break;
                }
            }
        }
        [BurstCompile]
        private float DistancePointToSegment(float3 point, float3 a, float3 b)
        {
            float3 ab = b - a;
            float3 ap = point - a;

            float t = math.saturate(math.dot(ap, ab) / math.dot(ab, ab));
            float3 closest = a + t * ab;
            return math.distance(point, closest);
        }

        [BurstCompile]
        private bool LineSegmentIntersectsAABB(float3 p0, float3 p1, float3 halfExtents)
        {
            float3 m = (p0 + p1) * 0.5f;
            float3 d = p1 - m;

            return math.abs(m.x) <= halfExtents.x + math.abs(d.x) &&
                   math.abs(m.y) <= halfExtents.y + math.abs(d.y);
        }
    }
}

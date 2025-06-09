using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SpaceGame.Combat
{
    public struct CollisionBasedCollector : ISpatialQueryCollector
    {
        public ComponentLookup<BoundingRadius> RadiusLookup;
        public BufferLookup<HitBoxElement> HitboxBufferLookup;
        public ComponentLookup<LocalToWorld> LtwLookup;

        public int OwnTeam;
        public LocalToWorld ltw;
        public DynamicBuffer<HitBoxElement> Hitboxes;
        public float Radius;
        public bool Hit;
        public Entity HitEntity;

        public CollisionBasedCollector(ComponentLookup<BoundingRadius> radiusLookup, BufferLookup<HitBoxElement> hitboxBufferLookup, ComponentLookup<LocalToWorld> ltwLookup
            , int ownTeam, LocalToWorld ltw, DynamicBuffer<HitBoxElement> hitboxes, float radius) : this()
        {
            RadiusLookup = radiusLookup;
            HitboxBufferLookup = hitboxBufferLookup;
            LtwLookup = ltwLookup;
            OwnTeam = ownTeam;
            this.ltw = ltw;
            Hitboxes = hitboxes;
            Radius = radius;

            HitEntity = Entity.Null;
            Hit = false;
        }

        public void OnVisitCell(in SpatialDatabaseCell cell, in UnsafeList<SpatialDatabaseElement> elements, out bool shouldEarlyExit)
        {
            shouldEarlyExit = false;

            for (int i = 0; i < cell.ElementsCount; i++)
            {
                var element = elements[cell.StartIndex + i];

                if (element.Team == OwnTeam)
                    continue;

                if (!HitboxBufferLookup.HasBuffer(element.Entity))
                    continue;

                if (!RadiusLookup.HasComponent(element.Entity))
                    continue;

                var targetRadius = RadiusLookup[element.Entity].Value;
                var distSq = math.distancesq(element.Position, ltw.Position);

                if (distSq > Radius * Radius + targetRadius * targetRadius)
                    continue;

                var targetHitboxes = HitboxBufferLookup[element.Entity];
                var targetLtw = LtwLookup[element.Entity];
                foreach (var hitbox in Hitboxes)
                {

                    float3 hitboxWorldCenter = ltw.Position + math.mul(ltw.Rotation, hitbox.LocalCenter * ltw.Value.Scale());
                    quaternion hitboxWorldRotation = math.mul(ltw.Rotation, hitbox.Rotation);
                    float3 halfExtents = hitbox.HalfExtents * ltw.Value.Scale();

                    foreach (var targetHitbox in targetHitboxes)
                    {
                        float3 targetHitboxWorldCenter = targetLtw.Position + math.mul(targetLtw.Rotation, targetHitbox.LocalCenter * targetLtw.Value.Scale());
                        quaternion targetHitboxWorldRotation = math.mul(targetLtw.Rotation, targetHitbox.Rotation);
                        float3 targetHalfExtents = targetHitbox.HalfExtents * targetLtw.Value.Scale();

                        bool overlap = PhysicsCustom.CheckOBBOverlap(hitboxWorldCenter, halfExtents, hitboxWorldRotation, targetHitboxWorldCenter, targetHalfExtents, targetHitboxWorldRotation);

                        if (overlap)
                        {
                            shouldEarlyExit = true;
                            Hit = true;
                            HitEntity = element.Entity;
                            return;
                        }
                    }
                }

                shouldEarlyExit = false;
            }
        }
    }
}
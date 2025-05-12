using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SpaceGame.Combat
{
    public struct RangeAndAngleTargetCollector : ISpatialQueryCollector
    {
        public float3 Position;
        public float3 Forward;
        public float MaxAngleCos;    
        public float MaxDistanceSq;
        public byte OwnTeam;

        public Entity FoundTarget;
        public BufferLookup<HitBoxElement> HitboxLookup;
        public ComponentLookup<LocalToWorld> LTWLookup;

        public void OnVisitCell(in SpatialDatabaseCell cell, in UnsafeList<SpatialDatabaseElement> elements, out bool shouldEarlyExit)
        {
            shouldEarlyExit = false;

            for (int i = 0; i < cell.ElementsCount; i++)
            {
                var element = elements[cell.StartIndex + i];

                if (element.Team == OwnTeam)
                    continue;

                //TODO: Currently this only checks if the center of the entity is in the desired angle, look for the hitboxes of the entity and see if any of these extents is inside of the desired angle

                float3 toTarget = element.Position - Position;
                float distSq = math.lengthsq(toTarget);

                if (distSq > MaxDistanceSq)
                    continue;

                float3 direction = math.normalize(toTarget);
                float dot = math.dot(Forward, direction);

                if (dot < MaxAngleCos)
                    continue;

                FoundTarget = element.Entity;
                shouldEarlyExit = true; 
                return;
            }
        }

        private void CheckEntityHitboxes(SpatialDatabaseElement element)
        {
            if (!HitboxLookup.HasBuffer(element.Entity))
                return;

            var hitboxes = HitboxLookup[element.Entity];
        }
    }
}
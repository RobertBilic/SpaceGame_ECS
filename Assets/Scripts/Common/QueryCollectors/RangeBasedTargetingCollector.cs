using SpaceGame.Combat.Components;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SpaceGame.Combat
{
    public struct RangeBasedTargetingCollector : ISpatialQueryCollector
    {
        public RangeBasedTargetingCollector(EntityManager manager, float3 position, float range, int team)
        {
            this.em = manager;
            this.collectedEnemy = Entity.Null;
            this.myTeamTag = team;
            this.myPosition = position;
            this.myRange = range;
        }

        public Entity collectedEnemy;

        private EntityManager em;
        private int myTeamTag;
        private float3 myPosition;
        private float myRange;

        public void OnVisitCell(in SpatialDatabaseCell cell, in UnsafeList<SpatialDatabaseElement> elements, out bool shouldEarlyExit)
        {
            shouldEarlyExit = false;

            if (cell.ElementsCount == 0)
                return;

            for (int i = cell.StartIndex; i < cell.StartIndex + cell.ElementsCount; i++)
            {
                var entity = elements[i].Entity;

                if (entity == Entity.Null || !em.Exists(entity))
                    continue;

                if (!em.HasComponent<IsAlive>(entity) || !em.HasComponent<TeamTag>(entity))
                    continue;

                var teamTag = em.GetComponentData<TeamTag>(entity);
                var isAlive = em.HasComponent<IsAlive>(entity);

                if (!isAlive)
                    continue;

                if (teamTag.Team == myTeamTag)
                    continue;

                if (!em.HasComponent<LocalToWorld>(entity))
                    continue;

                var targetPosition = em.GetComponentData<LocalToWorld>(entity);

                float distanceSq = math.distancesq(myPosition, targetPosition.Position);

                if (distanceSq > myRange * myRange)
                    continue;


                collectedEnemy = entity;
                shouldEarlyExit = true;
                break;
            }
        }
    }
}
using SpaceGame.Combat.Components;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SpaceGame.Combat
{
    public enum TeamFilterMode
    {
        DifferentTeam,
        SameTeam,
        All
    }

    public struct RangeBasedTargetingCollectorMultiple : ISpatialQueryCollector
    {
        public RangeBasedTargetingCollectorMultiple(ref NativeList<Entity> collectedEnemies, EntityManager manager, float3 position, float range, TeamFilterMode filterMode,int team)
        {
            this.em = manager;
            this.myTeamTag = team;
            this.myPosition = position;
            this.myRange = range;
            this.filterMode = filterMode;
            this.collectedEnemies = collectedEnemies;
        }

        public NativeList<Entity> collectedEnemies;

        private EntityManager em;
        private int myTeamTag;
        private float3 myPosition;
        private float myRange;
        private TeamFilterMode filterMode;

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

                bool isSameTeam = teamTag.Team == myTeamTag;
                if ((filterMode == TeamFilterMode.DifferentTeam && isSameTeam) ||
                    (filterMode == TeamFilterMode.SameTeam && !isSameTeam))
                    continue;

                if (!em.HasComponent<LocalToWorld>(entity))
                    continue;

                var targetPosition = em.GetComponentData<LocalToWorld>(entity);

                float distanceSq = math.distancesq(myPosition, targetPosition.Position);

                if (distanceSq > myRange * myRange)
                    continue;

                if(!collectedEnemies.Contains(entity))
                    collectedEnemies.Add(entity);
            }
        }
    }
}
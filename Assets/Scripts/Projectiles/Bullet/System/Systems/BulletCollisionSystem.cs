using SpaceGame.Combat.Components;
using SpaceGame.Combat.Jobs;
using SpaceGame.Combat.QueryCollectors;
using SpaceGame.Movement.Components;
using SpaceGame.SpatialGrid.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Transforms;

namespace SpaceGame.Combat.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(CombatCollisionGroup))]
    partial struct BulletCollisionSystem : ISystem
    {
        private ComponentLookup<SpatialDatabase> databaseLookup;
        private BufferLookup<SpatialDatabaseCell> cellbufferLookup;
        private BufferLookup<SpatialDatabaseElement> elementLookup;

        private ComponentLookup<TeamTag> teamLookup;
        private ComponentLookup<LocalToWorld> ltwLookup;
        private ComponentLookup<LocalTransform> ltLookup;
        private ComponentLookup<BoundingRadius> radiusLookup;
        private BufferLookup<HitBoxElement> hitboxLookup;

        private NativeList<CachedSpatialDatabaseRO> CachedDatabases;
        private bool listConstructed;

        public void OnCreate(ref SystemState state)
        {
            teamLookup = state.GetComponentLookup<TeamTag>(true);
            ltwLookup = state.GetComponentLookup<LocalToWorld>(true);
            ltLookup = state.GetComponentLookup<LocalTransform>(true);
            radiusLookup = state.GetComponentLookup<BoundingRadius>(true);
            hitboxLookup = state.GetBufferLookup<HitBoxElement>(true);

            databaseLookup = SystemAPI.GetComponentLookup<SpatialDatabase>(true);
            cellbufferLookup = SystemAPI.GetBufferLookup<SpatialDatabaseCell>(true);
            elementLookup = SystemAPI.GetBufferLookup<SpatialDatabaseElement>(true);

            state.RequireForUpdate<SpatialDatabaseSingleton>();
        }


        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingletonEntity<ProjectilePoolRequest>(out var projectilePoolEntity))
                return;
            SystemAPI.TryGetSingletonEntity<ImpactSpawnRequest>(out var impactSpawnRequestEntity);

            databaseLookup.Update(ref state);
            cellbufferLookup.Update(ref state);
            elementLookup.Update(ref state);

            if (!SystemAPI.TryGetSingleton<SpatialDatabaseSingleton>(out SpatialDatabaseSingleton spatialDatabaseSingleton))
                return;
            ref var databases = ref spatialDatabaseSingleton.TeamBasedDatabases.Value.TeamBasedDatabases;

            NativeList<CachedSpatialDatabaseRO> databaseList = TeamBasedSpatialDatabaseUtility.ConstructCachedSpatialDatabseROList(ref databases, databaseLookup, cellbufferLookup, elementLookup);

            teamLookup.Update(ref state);
            ltwLookup.Update(ref state);
            ltLookup.Update(ref state);
            radiusLookup.Update(ref state);
            hitboxLookup.Update(ref state);

            if (listConstructed)
                DisposeList();

            CachedDatabases = TeamBasedSpatialDatabaseUtility.ConstructCachedSpatialDatabseROList(spatialDatabaseSingleton, databaseLookup, cellbufferLookup,
                elementLookup, Allocator.TempJob);

            listConstructed = true;
            var numberOfTeams = CachedDatabases.Length;
            var numberOfJobsPerTeam = math.max(1, JobsUtility.JobWorkerMaximumCount / numberOfTeams);

            var initialDep = state.Dependency;

            for (int i = 0; i < CachedDatabases.Length; i++)
            {
                TeamBasedSpatialDatabaseUtility.GetTeamBasedDatabase(CachedDatabases, CachedDatabases[i].Team, TeamFilterMode.DifferentTeam, out var found, out var cachedDb);

                if (!found)
                    continue;

                for (int j = 0; j < numberOfJobsPerTeam; j++)
                {
                    var ecbSingleton = SystemAPI.GetSingleton<CollisionCommandBufferSystem.Singleton>();
                    var job = new BulletCollisionJob
                    {
                        NumberOfJobs = numberOfJobsPerTeam,
                        JobNumber = j,
                        Ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
                        CachedDatabase = cachedDb,
                        Team = CachedDatabases[i].Team,
                        hitboxLookup = hitboxLookup,
                        ImpactSpawnEntity = impactSpawnRequestEntity,
                        ProjectilePoolEntity = projectilePoolEntity,
                        ltLookup = ltLookup,
                        ltwLookup = ltwLookup,
                        radiusLookup = radiusLookup,
                        teamLookup = teamLookup
                    };

                    state.Dependency = JobHandle.CombineDependencies(state.Dependency, job.Schedule(initialDep));
                }
            }

        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            if (!listConstructed)
                return;

            DisposeList();
        }

        private void DisposeList()
        {
            foreach (var db in CachedDatabases)
                db.Dispose();
            CachedDatabases.Dispose();
        }
    }

}
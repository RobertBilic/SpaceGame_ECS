using SpaceGame.Combat.Components;
using SpaceGame.SpatialGrid.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;

namespace SpaceGame.Combat.Systems
{
   [UpdateInGroup(typeof(CombatTargetingGroup))]
    public partial struct ForwardWeaponTargetingSystem : ISystem
    {
        private ComponentLookup<SpatialDatabase> spatialDatabaseLookup;
        private BufferLookup<SpatialDatabaseCell> spatialDatabaseCellLookup;
        private BufferLookup<SpatialDatabaseElement> spatialDatabaseElementLookup;
        private ComponentLookup<ForwardWeapon> forwardWeaponLookup;
        private BufferLookup<HitBoxElement> hitboxElementLookup;

        private NativeList<CachedSpatialDatabaseRO> CachedDatabases;
        private bool listConstructed;

      public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ForwardWeapon>();

            spatialDatabaseLookup = SystemAPI.GetComponentLookup<SpatialDatabase>(true);
            spatialDatabaseCellLookup = SystemAPI.GetBufferLookup<SpatialDatabaseCell>(true);
            spatialDatabaseElementLookup = SystemAPI.GetBufferLookup<SpatialDatabaseElement>(true);
            forwardWeaponLookup = state.GetComponentLookup<ForwardWeapon>(true);
            hitboxElementLookup = state.GetBufferLookup<HitBoxElement>(true);
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<SpatialDatabaseSingleton>(out SpatialDatabaseSingleton spatialDatabaseSingleton))
                return;

            spatialDatabaseCellLookup.Update(ref state);
            spatialDatabaseElementLookup.Update(ref state);
            spatialDatabaseLookup.Update(ref state);

            forwardWeaponLookup.Update(ref state);
            hitboxElementLookup.Update(ref state);

            if (listConstructed)
                DisposeList();

            CachedDatabases = TeamBasedSpatialDatabaseUtility.ConstructCachedSpatialDatabseROList(spatialDatabaseSingleton, spatialDatabaseLookup, spatialDatabaseCellLookup,
                spatialDatabaseElementLookup, Allocator.TempJob);

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
                    var ecbSingleton = SystemAPI.GetSingleton<TargetingCommandBufferSystem.Singleton>();
                    var job = new ForwardWeaponTargetingJob
                    {
                        NumberOfJobs = numberOfJobsPerTeam,
                        JobNumber = j,
                        Ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
                        Database = cachedDb,
                        WeaponLookup = forwardWeaponLookup,
                        HitboxElement = hitboxElementLookup,
                        Team = CachedDatabases[i].Team
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
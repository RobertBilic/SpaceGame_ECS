using SpaceGame.Combat.Components;
using SpaceGame.SpatialGrid.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

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

            for (int i = 0; i < CachedDatabases.Length; i++)
            {
                TeamBasedSpatialDatabaseUtility.GetTeamBasedDatabase(CachedDatabases, CachedDatabases[i].Team, TeamFilterMode.DifferentTeam, out var found, out var cachedDb);

                if (!found)
                    continue;

                var job = new ForwardWeaponTargetingJob
                {
                    Database = cachedDb,
                    WeaponLookup = forwardWeaponLookup,
                    HitboxElement = hitboxElementLookup,
                    Team = CachedDatabases[i].Team
                };

                state.Dependency = job.ScheduleParallel(state.Dependency);
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
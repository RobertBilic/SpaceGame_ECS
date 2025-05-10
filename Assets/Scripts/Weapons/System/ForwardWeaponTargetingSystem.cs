using SpaceGame.Combat.Components;
using System;
using Unity.Entities;

namespace SpaceGame.Combat.Systems
{
   [UpdateInGroup(typeof(CombatTargetingGroup))]
    public partial struct ForwardWeaponTargetingSystem : ISystem
    {
        private CachedSpatialDatabaseRO _CachedSpatialDatabase;

        private ComponentLookup<SpatialDatabase> spatialDatabaseLookup;
        private BufferLookup<SpatialDatabaseCell> spatialDatabaseCellLookup;
        private BufferLookup<SpatialDatabaseElement> spatialDatabaseElementLookup;
        private ComponentLookup<ForwardWeapon> forwardWeaponLookup;


      public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ForwardWeapon>();

            spatialDatabaseLookup = SystemAPI.GetComponentLookup<SpatialDatabase>(true);
            spatialDatabaseCellLookup = SystemAPI.GetBufferLookup<SpatialDatabaseCell>(true);
            spatialDatabaseElementLookup = SystemAPI.GetBufferLookup<SpatialDatabaseElement>(true);
            forwardWeaponLookup = state.GetComponentLookup<ForwardWeapon>(true);
        }

        public void OnUpdate(ref SystemState state)
        {
            if (SystemAPI.TryGetSingleton<SpatialDatabaseSingleton>(out SpatialDatabaseSingleton spatialDatabaseSingleton))
            {
                spatialDatabaseCellLookup.Update(ref state);
                spatialDatabaseElementLookup.Update(ref state);
                spatialDatabaseLookup.Update(ref state);

                _CachedSpatialDatabase = new CachedSpatialDatabaseRO
                {
                    SpatialDatabaseEntity = spatialDatabaseSingleton.TargetablesSpatialDatabase,
                    SpatialDatabaseLookup = spatialDatabaseLookup,
                    CellsBufferLookup = spatialDatabaseCellLookup,
                    ElementsBufferLookup = spatialDatabaseElementLookup
                };

                _CachedSpatialDatabase.CacheData();
            }
            else
            {
                return;
            }

            forwardWeaponLookup.Update(ref state);

            var job = new ForwardWeaponTargetingJob
            {
                CachedDb = _CachedSpatialDatabase,
                WeaponLookup = forwardWeaponLookup
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);
        }
    }
}
using SpaceGame.Combat;
using SpaceGame.SpatialGrid.Components;
using System;
using Unity.Collections;
using Unity.Entities;

public static class TeamBasedSpatialDatabaseUtility
{
    public static NativeList<CachedSpatialDatabaseRO> ConstructCachedSpatialDatabseROList(ref BlobArray<TeamSpatialDatabase> array,ComponentLookup<SpatialDatabase> dbLookup
        , BufferLookup<SpatialDatabaseCell> cellElementLookup, BufferLookup<SpatialDatabaseElement> elementLookup,  Allocator allocator = Allocator.Temp)
    {
        NativeList<CachedSpatialDatabaseRO> list = new NativeList<CachedSpatialDatabaseRO>(array.Length,allocator);

        for (int i = 0; i < array.Length; i++)
        {
            var db = new CachedSpatialDatabaseRO()
            {
                SpatialDatabaseLookup = dbLookup,
                ElementsBufferLookup = elementLookup,
                CellsBufferLookup = cellElementLookup,
                Team = array[i].Team,
                SpatialDatabaseEntity = array[i].Database
            };
            db.CacheData();
            list.Add(db);
        }

        return list;
    }

    public static void GetTeamBasedDatabase(NativeList<CachedSpatialDatabaseRO> databases, int team, TeamFilterMode filterMode, out bool found, out CachedSpatialDatabaseRO database)
    {
        found = false;
        database = default(CachedSpatialDatabaseRO);

        for (int i = 0; i < databases.Length; i++)
        {
            bool isSameTeam = databases[i].Team == team;
            if ((filterMode == TeamFilterMode.DifferentTeam && isSameTeam) ||
                (filterMode == TeamFilterMode.SameTeam && !isSameTeam))
                continue;

            database = databases[i];
            found = true;
            return;
        }
    }

    public static NativeList<CachedSpatialDatabaseRO> ConstructCachedSpatialDatabseROList(SpatialDatabaseSingleton spatialDatabaseSingleton,
        ComponentLookup<SpatialDatabase> spatialDatabaseLookup, BufferLookup<SpatialDatabaseCell> spatialDatabaseCellLookup, 
        BufferLookup<SpatialDatabaseElement> spatialDatabaseElementLookup, Allocator allocator = Allocator.Temp)
    {
        ref var array = ref spatialDatabaseSingleton.TeamBasedDatabases.Value.TeamBasedDatabases;
        return ConstructCachedSpatialDatabseROList(ref array, spatialDatabaseLookup, spatialDatabaseCellLookup, spatialDatabaseElementLookup, allocator);
    }
}

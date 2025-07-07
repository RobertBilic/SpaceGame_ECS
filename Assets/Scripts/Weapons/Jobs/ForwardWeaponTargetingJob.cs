using SpaceGame.Combat;
using SpaceGame.Combat.Components;
using SpaceGame.SpatialGrid.Components;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial struct ForwardWeaponTargetingJob : IJobEntity
{
    [ReadOnly] 
    public CachedSpatialDatabaseRO Database;
    [ReadOnly]
    public ComponentLookup<ForwardWeapon> WeaponLookup;
    [ReadOnly]
    public BufferLookup<HitBoxElement> HitboxElement;
    public int Team;

    public EntityCommandBuffer.ParallelWriter Ecb;
    public int NumberOfJobs;
    public int JobNumber;

    public void Execute([ChunkIndexInQuery] int chunkIndex,
        Entity entity,
        in DynamicBuffer<ForwardWeaponElement> weaponRefs,
        in LocalToWorld localToWorld,
        in TeamTag team)
    {
        if (entity.Index % NumberOfJobs != JobNumber)
            return;

        if (team.Team != Team)
            return;

        if (weaponRefs.Length == 0)
        {
            Ecb.SetComponent<Target>(chunkIndex, entity, new Target() { Value = Entity.Null });
            return;
        }

        float maxRange = 0f;
        float maxAngle = 0;
        float3 heading = math.mul(localToWorld.Rotation, new float3(1.0f,0.0f,0.0f));
        heading.z = 0.0f;

        float3 position = localToWorld.Position;

        for (int i = 0; i < weaponRefs.Length; i++)
        {
            Entity weaponEntity = weaponRefs[i].Ref;

            if (!WeaponLookup.HasComponent(weaponEntity))
                continue;

            var weapon = WeaponLookup[weaponEntity];
            maxRange = math.max(maxRange, weapon.Range);
            maxAngle = math.max(maxAngle, weapon.MaxRotationAngleDeg + weapon.MaxRotationPadding);
        }

        if (maxRange <= 0f)
        {
            Ecb.SetComponent<Target>(chunkIndex, entity, new Target() { Value = Entity.Null });
            return;
        }

        var collector = new RangeAndAngleTargetCollector()
        {
            Forward = heading,
            MaxDistanceSq = maxRange * maxRange,
            MaxAngleCos = math.cos(math.radians(maxAngle)),
            OwnTeam = (byte)team.Team,
            Position = position
        };

        SpatialDatabase.QuerySphereCellProximityOrder(
            Database._SpatialDatabase,
            Database._SpatialDatabaseCells,
            Database._SpatialDatabaseElements,
            position,
            maxRange,
            ref collector
        );

        Ecb.SetComponent<Target>(chunkIndex, entity, new Target() { Value = collector.FoundTarget });
    }
}

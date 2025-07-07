using SpaceGame.Combat;
using SpaceGame.Combat.Components;
using SpaceGame.Combat.QueryCollectors;
using SpaceGame.Movement.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SpaceGame.Combat.Jobs
{
    [BurstCompile]
    public partial struct BulletCollisionJob : IJobEntity
    {
        public Entity ProjectilePoolEntity;
        public Entity ImpactSpawnEntity;

        public CachedSpatialDatabaseRO CachedDatabase;

        [ReadOnly]
        public ComponentLookup<TeamTag> teamLookup;
        [ReadOnly]
        public ComponentLookup<LocalToWorld> ltwLookup;
        [ReadOnly]
        public ComponentLookup<LocalTransform> ltLookup;
        [ReadOnly]
        public ComponentLookup<BoundingRadius> radiusLookup;
        [ReadOnly]
        public BufferLookup<HitBoxElement> hitboxLookup;

        public EntityCommandBuffer.ParallelWriter Ecb;
        public int NumberOfJobs;
        public int JobNumber;
        public int Team;

        public void Execute([ChunkIndexInQuery] int chunkIndex,
            Entity bulletEntity, in LocalTransform lt, in PreviousPosition previousPos, in Radius radiusComp, in Damage damage, in TeamTag teamTag,
            in ProjectileId projectileId)
        {
            if (Team != teamTag.Team)
                return;

            if (bulletEntity.Index % NumberOfJobs != JobNumber)
                return;

            float3 bulletStart = previousPos.Value;
            float3 bulletEnd = lt.Position;
            float radius = radiusComp.Value;

            var bulletCollisionDetector = new BulletCollisionDetector(teamLookup, ltwLookup, ltLookup, radiusLookup, hitboxLookup, teamTag.Team, bulletStart, bulletEnd, radius);
            SpatialDatabase.QuerySphereCellProximityOrder(CachedDatabase._SpatialDatabase, CachedDatabase._SpatialDatabaseCells, CachedDatabase._SpatialDatabaseElements, bulletEnd, radius, ref bulletCollisionDetector);

            if (bulletCollisionDetector.isEnemyHit)
            {
                Ecb.AppendToBuffer(chunkIndex, bulletCollisionDetector.HitEntity, new DamageHealthRequestBuffer()
                {
                    Source = bulletEntity,
                    SourcePosition = bulletStart,
                    Value = damage.Value,
                    DamageType = damage.Type
                });

                Ecb.AddComponent<NeedHealthUpdateTag>(chunkIndex, bulletCollisionDetector.HitEntity); 
                
                if(ImpactSpawnEntity != Entity.Null)
                {
                    Ecb.AppendToBuffer(chunkIndex, ImpactSpawnEntity, new ImpactSpawnRequest()
                    {
                        Count = 1,
                        Normal = math.up(),
                        Position = bulletEnd,
                        Scale = radius * 3.0f,
                        PrefabId = projectileId.Value
                    });
                }

                if (ProjectilePoolEntity != Entity.Null)
                {
                    Ecb.AppendToBuffer(chunkIndex, ProjectilePoolEntity, new ProjectilePoolRequest()
                    {
                        Entity = bulletEntity,
                        Id = projectileId.Value
                    });
                }
            }
        }
    }
}

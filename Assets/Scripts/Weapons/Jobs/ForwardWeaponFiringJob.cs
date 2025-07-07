using SpaceGame.Combat.Components;
using SpaceGame.Movement.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SpaceGame.Combat.Jobs
{
    [WithAll(typeof(ForwardWeaponElement))]
    public partial struct ForwardWeaponFiringJob : IJobEntity
    {

        public int JobCount;
        public int JobNumber;

        public float deltaTime;
        public double elapsedTime;

        public Entity ProjectileSpawnRequestEntity;

        [ReadOnly]
        public NativeList<ProjectilePrefab> ProjectilePrefabs;

        [ReadOnly]
        public BufferLookup<ForwardWeaponElement> ForwardWeaponLookup;
        [ReadOnly]
        public BufferLookup<ProjectileSpawnOffset> OffsetBufferLookup;
        [ReadOnly]
        public ComponentLookup<ForwardWeapon> WeaponLookup;
        [ReadOnly]
        public ComponentLookup<LocalToWorld> LtwLookup;
        [ReadOnly]
        public ComponentLookup<LocalTransform> LtLookup;

        public EntityCommandBuffer.ParallelWriter Ecb;

        public void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, in Target target, in TeamTag team)
        {
            if (target.Value == Entity.Null)
                return;
            
            if (entity.Index % JobCount != JobNumber)
                return;

            if (!LtwLookup.HasComponent(entity) || !LtwLookup.HasComponent(target.Value))
                return;


            var weaponBuffer = ForwardWeaponLookup[entity];
            float3 targetPos = LtwLookup[target.Value].Position;

            for (int i = 0; i < weaponBuffer.Length; i++)
            {
                Entity weaponEntity = weaponBuffer[i].Ref;

                if (!WeaponLookup.HasComponent(weaponEntity))
                    continue;

                var weapon = WeaponLookup[weaponEntity];

                ProjectilePrefab prefabData = default(ProjectilePrefab);

                for(int j = 0; j < ProjectilePrefabs.Length; j++)
                {
                    if(ProjectilePrefabs[j].Id == weapon.BulletId)
                    {
                        prefabData = ProjectilePrefabs[j];
                        break;
                    }    
                }

                if (prefabData.Entity == Entity.Null)
                    continue;

                if (!LtLookup.HasComponent(weapon.RotationBase) ||
                    !LtwLookup.HasComponent(weapon.RotationBase))
                    continue;

                var baseLtw = LtwLookup[weapon.RotationBase];
                float3 weaponPos = baseLtw.Position;

                float3 toTargetRaw = targetPos - weaponPos;
                if (math.lengthsq(toTargetRaw) < 0.0001f)
                    continue;

                float3 toTarget = math.normalize(toTargetRaw);
                float3 baseForward = math.mul(baseLtw.Rotation, new float3(1, 0, 0));

                if (!math.isfinite(baseForward.x) || !math.isfinite(baseForward.y))
                    continue;

                float3 flatForward = new float3(baseForward.x, baseForward.y, 0);
                if (math.lengthsq(flatForward) < 0.0001f)
                    continue;

                float3 flatToTarget = new float3(toTarget.x, toTarget.y, 0);
                if (math.lengthsq(flatToTarget) < 0.0001f)
                    continue;

                flatForward = math.normalize(flatForward);
                flatToTarget = math.normalize(flatToTarget);

                float angleToTargetDeg = math.degrees(math.atan2(flatToTarget.y, flatToTarget.x) -
                                      math.atan2(flatForward.y, flatForward.x));

                angleToTargetDeg = NormalizeAngleDegrees(angleToTargetDeg);

                float maxStep = weapon.RotationSpeed * deltaTime;
                float clampedStep = math.clamp(angleToTargetDeg, -maxStep, maxStep);

                weapon.CurrentRotationOffsetDeg = math.clamp(
                    weapon.CurrentRotationOffsetDeg + clampedStep,
                    -weapon.MaxRotationAngleDeg,
                    weapon.MaxRotationAngleDeg);

                if (LtLookup.HasComponent(weapon.RotationBase))
                {
                    var localTransform = LtLookup[weapon.RotationBase];
                    quaternion rotation = quaternion.RotateZ(math.radians(weapon.CurrentRotationOffsetDeg));
                    localTransform.Rotation = rotation;
                    Ecb.SetComponent(chunkIndex, weapon.RotationBase, localTransform);
                }

                float3 finalForward = math.mul(quaternion.RotateZ(math.radians(weapon.CurrentRotationOffsetDeg)), flatForward);
                finalForward.z = 0.0f;


                float alignment = math.dot(finalForward, flatToTarget);
                float maxDot = math.cos(math.radians(weapon.MaxRotationAngleDeg));

                if (alignment < 0.95)
                    continue;

                if ((elapsedTime - weapon.LastFireTime) < (1f / weapon.FiringRate))
                    continue;

                weapon.LastFireTime = (float)elapsedTime;
                Ecb.SetComponent(chunkIndex, weaponEntity, weapon);

                if (!OffsetBufferLookup.HasBuffer(weaponEntity))
                    continue;

                var spawnOffsets = OffsetBufferLookup[weaponEntity];

                foreach (var offset in spawnOffsets)
                {
                    float3 spawnPosition = math.transform(baseLtw.Value, offset.Value);

                    Ecb.AppendToBuffer(chunkIndex, ProjectileSpawnRequestEntity, new ProjectileSpawnRequest()
                    {
                        Target = target.Value,
                        BulletId = weapon.BulletId,
                        Damage = weapon.Damage,
                        DamageType = weapon.DamageType,
                        Heading = finalForward,
                        Position = spawnPosition,
                        Range = weapon.Range,
                        Team = team.Team,
                        ParentScale = math.length(baseLtw.Value.c0.xyz)
                    });
                }
            }
        }

        private float NormalizeAngleDegrees(float angle)
        {
            angle %= 360f;
            if (angle > 180f)
                angle -= 360f;
            else if (angle < -180f)
                angle += 360f;
            return angle;
        }
    }
}

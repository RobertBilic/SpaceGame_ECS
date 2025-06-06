using Unity.Entities;
using Unity.Transforms;

namespace SpaceGame.Combat.Components
{
    public readonly partial struct TurretFiringAspect : IAspect
    {
        public readonly Entity Entity;

        public readonly RefRO<LocalToWorld> WorldTransform;
        public readonly RefRO<Target> Target;
        public readonly RefRO<FiringRate> FiringRate;
        public readonly RefRW<LastFireTime> LastFireTime;
        public readonly RefRO<IsAlive> IsAlive;
        public readonly RefRO<Heading> Heading;
        public readonly RefRO<Range> Range;
        public readonly RefRO<Damage> Damage;
        public readonly RefRO<RotationBaseReference> RotationBaseReference;
        public readonly RefRO<TeamTag> TeamTag;
        public readonly RefRO<ProjectileId> BulletId;

        public readonly DynamicBuffer<ProjectileSpawnOffset> BulletSpawnOffsets;
    }
}
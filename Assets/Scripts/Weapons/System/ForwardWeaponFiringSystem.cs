using SpaceGame.Combat.Components;
using SpaceGame.Combat.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Transforms;

namespace SpaceGame.Combat.Systems
{
    [UpdateInGroup(typeof(CombatFiringGroup))]
    [BurstCompile]
    public partial struct ForwardWeaponFiringSystem : ISystem
    {
        private ComponentLookup<ForwardWeapon> weaponLookup;
        private ComponentLookup<LocalTransform> localTransformLookup;
        private ComponentLookup<LocalToWorld> localToWorldLookup;
        private BufferLookup<ProjectileSpawnOffset> offsetBufferLookup;
        private BufferLookup<ForwardWeaponElement> forwardWeaponElementLookup;

        private NativeList<ProjectilePrefab> prefabList;
        private bool prefabListConstructed;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            prefabListConstructed = false;
            state.RequireForUpdate<Target>();
            state.RequireForUpdate<ForwardWeaponElement>();

            weaponLookup = state.GetComponentLookup<ForwardWeapon>(true);
            localTransformLookup = state.GetComponentLookup<LocalTransform>(true);
            localToWorldLookup = state.GetComponentLookup<LocalToWorld>(true);
            offsetBufferLookup = state.GetBufferLookup<ProjectileSpawnOffset>(true);
            forwardWeaponElementLookup = state.GetBufferLookup<ForwardWeaponElement>(true);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            if (!prefabListConstructed)
                return;

            prefabList.Dispose();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingleton<ProjectilePrefabLookupSingleton>(out var blobSingleton))
                return;

            if (!SystemAPI.TryGetSingleton<GlobalTimeComponent>(out var timeComp))
                return;

            if (!SystemAPI.TryGetSingletonEntity<ProjectileSpawnRequest>(out var projectileSpawnRequestCollector))
                return;

            weaponLookup.Update(ref state);
            localTransformLookup.Update(ref state);
            localToWorldLookup.Update(ref state);
            offsetBufferLookup.Update(ref state);
            forwardWeaponElementLookup.Update(ref state);

            if (!prefabListConstructed)
            {
                ref var lookup = ref blobSingleton.Lookup.Value;
                prefabList = new NativeList<ProjectilePrefab>(lookup.Entries.Length, Allocator.Persistent);

                for(int i = 0; i < lookup.Entries.Length; i++)
                {
                    prefabList.Add(lookup.Entries[i]);
                }
            }
            var elapsedTime = timeComp.ElapsedTimeScaled;
            var deltaTime = timeComp.DeltaTimeScaled;

            var initialDep = state.Dependency;
            var maxJobs = math.max(1,JobsUtility.JobWorkerMaximumCount);

            for(int i = 0; i < maxJobs; i++)
            {
                var ecbSingleton = SystemAPI.GetSingleton<FiringCommandBufferSystem.Singleton>();

                var job = new ForwardWeaponFiringJob()
                {
                    deltaTime = deltaTime,
                    elapsedTime = elapsedTime,
                    ProjectileSpawnRequestEntity = projectileSpawnRequestCollector,
                    Ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
                    JobCount = maxJobs,
                    JobNumber = i,
                    WeaponLookup = weaponLookup,
                    ProjectilePrefabs = prefabList,
                    OffsetBufferLookup = offsetBufferLookup,
                    LtLookup = localTransformLookup,
                    LtwLookup = localToWorldLookup,
                    ForwardWeaponLookup = forwardWeaponElementLookup
                };

                state.Dependency = JobHandle.CombineDependencies(state.Dependency, job.Schedule(initialDep));
            }
        }
    }
}
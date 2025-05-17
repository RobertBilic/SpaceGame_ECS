using SpaceGame.Combat.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace SpaceGame.Combat.Systems
{
    [UpdateInGroup(typeof(CombatLateUpdateGroup))]
    [UpdateBefore(typeof(BulletFactorySystem))]
    partial struct BulletPoolSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BulletPoolRequest>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {

            if (!SystemAPI.TryGetSingletonBuffer<BulletPoolRequest>(out var bulletPoolCollector))
                return;

            if (bulletPoolCollector.Length != 0)
            {
                var poolEcb = new EntityCommandBuffer(Allocator.Temp);

                foreach (var request in bulletPoolCollector)
                {
                    ReturnEntityToPool(poolEcb, state.EntityManager, request.Entity, request.Id);
                }

                bulletPoolCollector.Clear();
                poolEcb.Playback(state.EntityManager);
                poolEcb.Dispose();
            }
        }

        void ReturnEntityToPool(EntityCommandBuffer ecb, EntityManager em, Entity entity, FixedString32Bytes id)
        {
            DisableEntityAndChildren(ecb, entity, em);
        }

        void DisableEntityAndChildren(EntityCommandBuffer ecb, Entity root, EntityManager em)
        {
            if (em.HasBuffer<LinkedEntityGroup>(root))
            {
                var group = em.GetBuffer<LinkedEntityGroup>(root);
                for (int i = 0; i < group.Length; i++)
                {
                    ecb.AddComponent<Disabled>(group[i].Value);
                    ecb.AddComponent<NeedsPoolingTag>(group[i].Value);
                }
            }
            else
            {
                ecb.AddComponent<NeedsPoolingTag>(root);
                ecb.AddComponent<Disabled>(root);
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}
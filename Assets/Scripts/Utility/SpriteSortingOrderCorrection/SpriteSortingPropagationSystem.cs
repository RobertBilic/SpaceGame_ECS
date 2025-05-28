using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace SpaceGame.Utility
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class SpriteSortingPropagationSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var entityManager = EntityManager;

            var rootQuery = SystemAPI.QueryBuilder()
                .WithAll<SpriteSortingRoot, Child>()
                .Build();

            var entityArray = rootQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            foreach (var rootEntity in entityArray)
            {
                if (!entityManager.HasComponent<SpriteSortingRoot>(rootEntity))
                    continue;

                int baseOrder = entityManager.GetComponentData<SpriteSortingRoot>(rootEntity).BaseOrder;
                ecb.RemoveComponent<SpriteSortingRoot>(rootEntity);
                PropagateSortingOrder(entityManager, rootEntity, baseOrder);
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
            entityArray.Dispose();
        }

        private void PropagateSortingOrder(EntityManager em, Entity entity, int baseOrder)
        {
            if (em.HasComponent<SpriteRenderer>(entity))
            {
                var sr = em.GetComponentObject<SpriteRenderer>(entity);
                sr.sortingOrder = baseOrder + sr.sortingOrder;
            }

            if (em.HasComponent<Child>(entity))
            {
                var buffer = em.GetBuffer<Child>(entity);
                foreach (var child in buffer)
                {
                    PropagateSortingOrder(em, child.Value, baseOrder);
                }
            }
        }
    }
}
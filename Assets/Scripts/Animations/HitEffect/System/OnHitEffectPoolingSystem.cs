using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(CombatLateUpdateGroup))]
[UpdateBefore(typeof(ImpactEffectSpawnSystem))]
partial struct OnHitEffectPoolingSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

        if (!SystemAPI.TryGetSingletonBuffer<ImpactEffectPoolRequest>(out var impactPoolCollector))
            return;

        if (impactPoolCollector.Length != 0)
        {
            var poolEcb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var request in impactPoolCollector)
            {
                ReturnEntityToPool(poolEcb, state.EntityManager, request.Entity, request.Id);
            }

            impactPoolCollector.Clear();
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

using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(CombatLateUpdateGroup), OrderFirst = true)]
partial struct HealthDamageSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NeedHealthUpdateTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach(var (health, damageRequest, entity) in SystemAPI.Query<RefRW<Health>, DynamicBuffer<DamageHealthRequestBuffer>>()
            .WithAll<NeedHealthUpdateTag>()
            .WithEntityAccess())
        {
            float healthBeforeUpdate = health.ValueRO.Current;

            foreach (var damage in damageRequest)
                health.ValueRW.Current -= damage.Value;

            float healthChange = healthBeforeUpdate - health.ValueRO.Current;

            if(SystemAPI.HasComponent<HealthBarReference>(entity))
            {
                var healthBarReference = SystemAPI.GetComponentRO<HealthBarReference>(entity).ValueRO;

                if (!state.EntityManager.IsEnabled(healthBarReference.Value))
                {
                    ecb.SetEnabled(healthBarReference.Value, true);
                }
                else
                {
                    var materialFill = SystemAPI.GetComponentRW<FillMaterialOverrideComponent>(healthBarReference.Value);
                    materialFill.ValueRW.Value = health.ValueRO.Current / health.ValueRO.Max;
                }
            }

            damageRequest.Clear();
            ecb.RemoveComponent<NeedHealthUpdateTag>(entity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}

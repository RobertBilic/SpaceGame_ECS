using SpaceGame.Combat.Components;
using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(CombatLateUpdateGroup), OrderFirst = true)]
partial struct HealthDamageSystem : ISystem
{
    ComponentLookup<TeamTag> teamLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NeedHealthUpdateTag>();
        teamLookup = state.GetComponentLookup<TeamTag>(true);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        teamLookup.Update(ref state);
        foreach(var (health, damageRequest, entity) in SystemAPI.Query<RefRW<Health>, DynamicBuffer<DamageHealthRequestBuffer>>()
            .WithAll<NeedHealthUpdateTag>()
            .WithEntityAccess())
        {
            foreach (var damage in damageRequest)
                health.ValueRW.Current -= damage.Value;

            if(SystemAPI.HasComponent<HealthBarReference>(entity))
            {
                var healthBarReference = SystemAPI.GetComponentRO<HealthBarReference>(entity).ValueRO;

                if(teamLookup.HasComponent(entity) && !teamLookup.HasComponent(healthBarReference.Value))
                {
                    ecb.AddComponent(healthBarReference.Value, new TeamTag() { Team = teamLookup[entity].Team });
                }

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

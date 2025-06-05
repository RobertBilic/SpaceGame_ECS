using SpaceGame.Combat.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[UpdateInGroup(typeof(CombatLateUpdateGroup))]
[UpdateAfter(typeof(HealthDamageSystem))]
partial struct HealthBarRecoloringSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<HealthBarColorPerTeam>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.TryGetSingletonBuffer<HealthBarColorPerTeam>(out var colorBuffer, true))
            return;

        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach (var (colorMaterialOverride, teamTag, entity) in SystemAPI.Query<RefRW<ColorMaterialOverride>, RefRO<TeamTag>>()
            .WithAll<NeedsHealthBarRecoloring>()
            .WithEntityAccess())
        {
            ecb.RemoveComponent<NeedsHealthBarRecoloring>(entity);

            bool found = false;
            float4 color = float4.zero;

            foreach(var data in colorBuffer)
            {
                if (data.Team != teamTag.ValueRO.Team)
                    continue;

                color = data.Color;
                found = true;
            }

            if (!found)
                continue;

            colorMaterialOverride.ValueRW.Value = color;
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {

    }
}

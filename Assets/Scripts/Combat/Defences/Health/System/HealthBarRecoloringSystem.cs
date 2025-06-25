using SpaceGame.Combat;
using SpaceGame.Combat.Components;
using SpaceGame.Combat.Defences;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[UpdateInGroup(typeof(CombatLateUpdateGroup))]
[UpdateAfter(typeof(HealthDamageSystem))]
partial struct HealthBarRecoloringSystem : ISystem
{
    NativeList<DefenceLayerType> orderedLayerTypes;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        orderedLayerTypes = DefenceLayerTypeUtility.GetOrderedDefenceLayerList(Allocator.Persistent);
        state.RequireForUpdate<HealthBarColorPerDefenceLayer>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (!SystemAPI.TryGetSingletonBuffer<HealthBarColorPerDefenceLayer>(out var colorBuffer, true))
            return;

        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        foreach (var (colorMaterialOverride, backgroundColorMaterialOverride, activeDefenceLayer, entity) in SystemAPI.Query<RefRW<ColorMaterialOverride>,
            RefRW<BackgroundColorMaterialOverride>, RefRO<ActiveDefenceLayer>>()
            .WithAll<NeedsHealthBarRecoloring>()
            .WithEntityAccess())
        {
            ecb.RemoveComponent<NeedsHealthBarRecoloring>(entity);

            float4 foregroundColor = GetColorForLayer(colorBuffer, activeDefenceLayer.ValueRO.Value);
            float4 backgroundColor = float4.zero;
            bool foundForeground = false;

            for(int i = 0; i < orderedLayerTypes.Length; i++)
            {
                if(orderedLayerTypes[i] == activeDefenceLayer.ValueRO.Value)
                {
                    foundForeground = true;
                }
                else
                {
                    if (foundForeground)
                    {
                        backgroundColor = GetColorForLayer(colorBuffer, orderedLayerTypes[i]);
                        break;
                    }
                }
            }
        
            backgroundColorMaterialOverride.ValueRW.Value = backgroundColor;
            colorMaterialOverride.ValueRW.Value = foregroundColor;
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    private float4 GetColorForLayer(DynamicBuffer<HealthBarColorPerDefenceLayer> colorBuffer, DefenceLayerType defenceLayerType)
    {

        foreach (var colorPerLayer in colorBuffer)
        {
            if (colorPerLayer.Layer != defenceLayerType)
                continue;

            return colorPerLayer.Color;
        }

        return float4.zero;
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        orderedLayerTypes.Dispose();
    }
}

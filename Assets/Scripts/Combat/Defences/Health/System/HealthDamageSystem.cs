using SpaceGame;
using SpaceGame.Combat.Components;
using SpaceGame.Combat.Defences;
using System;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[UpdateInGroup(typeof(CombatLateUpdateGroup), OrderFirst = true)]
partial struct HealthDamageSystem : ISystem
{
    ComponentLookup<TeamTag> teamLookup;
    ComponentLookup<HealthBarReference> healthBarReferenceLookup;
    NativeList<DefenceLayerType> orderedLayerTypes;
    NativeHashSet<Entity> addedHealthBarRecoloring;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NeedHealthUpdateTag>();
        addedHealthBarRecoloring = new NativeHashSet<Entity>(124, Allocator.Persistent);
        healthBarReferenceLookup = state.GetComponentLookup<HealthBarReference>(true);
        teamLookup = state.GetComponentLookup<TeamTag>(true);
        orderedLayerTypes = DefenceLayerTypeUtility.GetOrderedDefenceLayerList(Allocator.Persistent);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        teamLookup.Update(ref state);
        healthBarReferenceLookup.Update(ref state);
        addedHealthBarRecoloring.Clear();

        foreach(var (defenceLayers, resistances,damageRequest, entity) in SystemAPI.Query<DynamicBuffer<DefenceLayer>, DynamicBuffer<ResistanceEntry>, DynamicBuffer<DamageHealthRequestBuffer>>()
            .WithAll<NeedHealthUpdateTag>()
            .WithEntityAccess())
        {
            bool allDefencesDestroyed = false;
            float lastUpdateValue = 1.0f;
            float lastUpdateMax = 1.0f;
            DefenceLayerType activeDefenceLayer = DefenceLayerType.Shield;

            foreach (var dmg in damageRequest)
            {
                if (allDefencesDestroyed)
                    break;

                float remaining = dmg.Value;

                foreach (DefenceLayerType layerType in orderedLayerTypes)
                {
                    int layerIndex = FindLayerIndex(defenceLayers, layerType);
                    if (layerIndex < 0) continue;
                    ref var layer = ref defenceLayers.ElementAt(layerIndex);

                    if (layer.Value < float.Epsilon)
                        continue;

                    float resist = GetResistance(resistances, layerType, dmg.DamageType);
                    float reduced = remaining * (1 - resist);
                    float absorbed = math.min(layer.Value, reduced);
                    layer.Value -= absorbed;
                    remaining -= absorbed / (1 - resist);

                    lastUpdateValue = layer.Value;
                    lastUpdateMax = layer.Max;

                    activeDefenceLayer = layerType;

                    if (remaining <= 0)
                        break;

                    if (layer.Value < float.Epsilon)
                    {
                        if (!addedHealthBarRecoloring.Contains(entity))
                        {
                            if (healthBarReferenceLookup.HasComponent(entity))
                            {
                                var healthBarReference = healthBarReferenceLookup[entity];
                                ecb.AddComponent<NeedsHealthBarRecoloring>(healthBarReference.Value);
                            }

                            addedHealthBarRecoloring.Add(entity);
                        }
                    }
                }
            }


            bool hasNotDestroyedLayer = false;
            for (int i = 0; i < defenceLayers.Length && !hasNotDestroyedLayer; i++)
            {
                if (defenceLayers[i].Value >= float.Epsilon)
                {
                    hasNotDestroyedLayer = true;
                }
            }

            allDefencesDestroyed = !hasNotDestroyedLayer;

            if (healthBarReferenceLookup.HasComponent(entity))
            {
                var healthBarReference = healthBarReferenceLookup[entity];

                ecb.SetComponent(healthBarReference.Value, new ActiveDefenceLayer() { Value = activeDefenceLayer });

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
                    materialFill.ValueRW.Value = lastUpdateValue / lastUpdateMax;
                }
            }

            damageRequest.Clear();

            if (allDefencesDestroyed)
                ecb.AddComponent<PendingDestructionTag>(entity);

            ecb.RemoveComponent<NeedHealthUpdateTag>(entity);
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }

    private int FindLayerIndex(DynamicBuffer<DefenceLayer> layers, DefenceLayerType type)
    {
        for (int i = 0; i < layers.Length; i++)
            if (layers[i].Type == type)
                return i;
        return -1;
    }

    private float GetResistance(DynamicBuffer<ResistanceEntry> resistances, DefenceLayerType layer, DamageType type)
    {
        for (int i = 0; i < resistances.Length; i++)
        {
            if (resistances[i].Layer == layer && resistances[i].Type == type)
                return resistances[i].Resistance;
        }

        return 0f;
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        orderedLayerTypes.Dispose();
        addedHealthBarRecoloring.Dispose();
    }
}

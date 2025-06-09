using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SpaceGame.Combat.Systems
{
    [UpdateInGroup(typeof(CombatSystemGroup))]
    [UpdateAfter(typeof(CombatFiringGroup))]
    public partial struct MissileLoadedIndicatorSystem : ISystem
    {
        ComponentLookup<LocalTransform> ltLookup;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            ltLookup = state.GetComponentLookup<LocalTransform>();
            state.RequireForUpdate<ProjectileReloadData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            ltLookup.Update(ref state);

            foreach(var (ltw, reloadData, missileIndicator) in SystemAPI.Query<RefRO<LocalToWorld>, RefRO<ProjectileReloadData>, DynamicBuffer<MissileLoadedIndicator>>())
            {
                if (missileIndicator.Length == 0)
                    continue;

                var t = 1.0f - math.clamp(math.unlerp(0.0f, reloadData.ValueRO.ReloadTime, reloadData.ValueRO.CurrentReloadTime), 0.0f, 1.0f);

                foreach(var indicator in missileIndicator)
                {
                    if (!ltLookup.HasComponent(indicator.Entity))
                        continue;

                    var loadedPosition = indicator.LoadedPosition;
                    var unloadedPosition = indicator.UnloadedPosition;

                    var indicatorLt = ltLookup[indicator.Entity];
                    indicatorLt.Position = math.lerp(unloadedPosition, loadedPosition, t);
                    SystemAPI.SetComponent(indicator.Entity, indicatorLt);
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}

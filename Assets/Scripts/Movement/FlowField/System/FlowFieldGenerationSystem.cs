using SpaceGame.Movement.Flowfield.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SpaceGame.Movement.Flowfield.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct FlowFieldGenerationSystem : ISystem
    {
        EntityQuery capitalShipQuery;
        EntityQuery flowFieldQuery;

        float3 lastTargetPosition;
        float updateCooldown;
        const float UpdateInterval = 1.0f;
        const float PositionThreshold = 1.0f;

        public void OnCreate(ref SystemState state)
        {
            capitalShipQuery = state.GetEntityQuery(ComponentType.ReadOnly<CapitalShipTag>(), ComponentType.ReadOnly<LocalTransform>());
            flowFieldQuery = state.GetEntityQuery(ComponentType.ReadWrite<FlowFieldSettings>(), ComponentType.ReadWrite<FlowFieldCell>());
            updateCooldown = 0f;
        }

        public void OnUpdate(ref SystemState state)
        {
            if (capitalShipQuery.IsEmptyIgnoreFilter || flowFieldQuery.IsEmptyIgnoreFilter)
                return;

            if (!SystemAPI.TryGetSingleton<GlobalTimeComponent>(out var timeComp))
                return;

            var capitalShip = capitalShipQuery.GetSingletonEntity();
            var shipTransform = state.EntityManager.GetComponentData<LocalTransform>(capitalShip);
            float3 targetPosition = shipTransform.Position;

            updateCooldown -= timeComp.DeltaTime;
            if (updateCooldown > 0f)
                return;

            if (math.distance(targetPosition, lastTargetPosition) < PositionThreshold)
                return;

            updateCooldown = UpdateInterval;
            lastTargetPosition = targetPosition;

            var flowFieldEntity = flowFieldQuery.GetSingletonEntity();
            var settings = state.EntityManager.GetComponentData<FlowFieldSettings>(flowFieldEntity);
            var buffer = state.EntityManager.GetBuffer<FlowFieldCell>(flowFieldEntity);

            int gridX = (int)math.ceil(settings.WorldSize.x / settings.CellSize);
            int gridY = (int)math.ceil(settings.WorldSize.y / settings.CellSize);

            if (buffer.Length != gridX * gridY)
            {
                buffer.Clear();
                for (int i = 0; i < gridX * gridY; i++)
                {
                    buffer.Add(new FlowFieldCell { Direction = float2.zero });
                }
            }

            for (int y = 0; y < gridY; y++)
            {
                for (int x = 0; x < gridX; x++)
                {
                    float2 worldPos = new float2(
                        (x + 0.5f) * settings.CellSize - settings.WorldSize.x * 0.5f,
                        (y + 0.5f) * settings.CellSize - settings.WorldSize.y * 0.5f
                    );

                    float2 dirToTarget = math.normalize(new float2(targetPosition.x, targetPosition.y) - worldPos);

                    buffer[y * gridX + x] = new FlowFieldCell { Direction = dirToTarget };
                }
            }
        }
    }
}
using SpaceGame.Movement.Flowfield.Components;
using SpaceGame.Movement.Flowfield.Jobs;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace SpaceGame.Movement.Flowfield.Systems
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class ShipMovementSystem : SystemBase
    {
        private EntityQuery flowFieldQuery;
        private EntityQuery capitalShipQuery;
        private EndSimulationEntityCommandBufferSystem ecbSystem;

        protected override void OnCreate()
        {
            flowFieldQuery = GetEntityQuery(
                ComponentType.ReadOnly<FlowFieldSettings>(),
                ComponentType.ReadOnly<FlowFieldCell>());

            capitalShipQuery = GetEntityQuery(
                ComponentType.ReadOnly<CapitalShipTag>(),
                ComponentType.ReadOnly<LocalToWorld>());

            RequireForUpdate<CapitalShipTag>();
            RequireForUpdate<ApproachDistance>();
            RequireForUpdate(flowFieldQuery);

            ecbSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            if (flowFieldQuery.IsEmptyIgnoreFilter || capitalShipQuery.IsEmptyIgnoreFilter)
                return;

            var flowEntity = flowFieldQuery.GetSingletonEntity();
            var flowSettings = SystemAPI.GetComponent<FlowFieldSettings>(flowEntity);
            var buffer = SystemAPI.GetBuffer<FlowFieldCell>(flowEntity).ToNativeArray(Allocator.TempJob);

            var capTransform = SystemAPI.GetComponent<LocalToWorld>(capitalShipQuery.GetSingletonEntity());
            float3 capPos = capTransform.Position;

            int gx = (int)math.ceil(flowSettings.WorldSize.x / flowSettings.CellSize);
            int gy = (int)math.ceil(flowSettings.WorldSize.y / flowSettings.CellSize);

            var disengageLookup = GetComponentLookup<DisengageCurveDirection>(false);
            var bankingLookup = GetComponentLookup<ShipBankingData>(false);
            var random = new Unity.Mathematics.Random(32123);

            var job = new ShipFlowFieldMovementJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
                CapitalPosition = capPos,
                GridX = gx,
                GridY = gy,
                WorldSize = flowSettings.WorldSize.xy,
                CellSize = flowSettings.CellSize,
                FlowField = buffer,
                DisengageLookup = disengageLookup,
                BankingLookup = bankingLookup,
                ECB = ecbSystem.CreateCommandBuffer().AsParallelWriter(),
                Random = random
            };

            Dependency = job.ScheduleParallel(Dependency);
            ecbSystem.AddJobHandleForProducer(Dependency);
            buffer.Dispose(Dependency);
        }
    }
}
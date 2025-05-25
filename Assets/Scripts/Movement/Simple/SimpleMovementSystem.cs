using SpaceGame.Movement.Components;
using SpaceGame.Movement.Simple.Jobs;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace SpaceGame.Movement.Simple.Systems
{
    [UpdateInGroup(typeof(CombatMovementGroup))]
    public partial class SimpleShipMovementSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem ecbSystem;

        protected override void OnCreate()
        {
            RequireForUpdate<ApproachDistance>();

            ecbSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            if (!SystemAPI.TryGetSingleton<GlobalTimeComponent>(out var timeComp))
                return;

            var disengageLookup = GetComponentLookup<DisengageCurveDirection>(false);
            var bankingLookup = GetComponentLookup<ShipBankingData>(false);
            var random = new Unity.Mathematics.Random(32123);

            var job = new SimpleMovementJob
            {
                DeltaTime = timeComp.DeltaTimeScaled,
                DisengageLookup = disengageLookup,
                BankingLookup = bankingLookup,
                ECB = ecbSystem.CreateCommandBuffer().AsParallelWriter(),
                Random = random
            };

            Dependency = job.ScheduleParallel(Dependency);
            ecbSystem.AddJobHandleForProducer(Dependency);
            Dependency.Complete();
        }
    }
}
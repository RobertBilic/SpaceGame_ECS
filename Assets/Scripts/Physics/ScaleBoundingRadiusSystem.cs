using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Assets.Scripts.Physics
{
    public partial struct ScaleBoundingRadiusSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            foreach(var (ltw, boundingRadius, entity) in SystemAPI.Query<RefRO<LocalToWorld>, RefRW<BoundingRadius>>()
                .WithAll<NeedsBoundingRadiusScalingTag>()
                .WithEntityAccess())
            {
                boundingRadius.ValueRW.Value = boundingRadius.ValueRO.Value * math.length(ltw.ValueRO.Value.c0.xyz);
                ecb.RemoveComponent<NeedsBoundingRadiusScalingTag>(entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}

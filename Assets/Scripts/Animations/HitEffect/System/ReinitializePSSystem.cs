using Unity.Burst;
using Unity.Entities;

namespace SpaceGame.Animations.Systems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    partial struct ReinitializePSSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<AutoplayParticleSystem>();
            state.RequireForUpdate<ParticleSystemPlaying>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            foreach (var (particleSystem, entity) in SystemAPI.Query<SystemAPI.ManagedAPI.UnityEngineComponent<UnityEngine.ParticleSystem>>()
                .WithAll<AutoplayParticleSystem,Disabled,ParticleSystemPlaying>()
                .WithEntityAccess())
            {
                ecb.RemoveComponent<ParticleSystemPlaying>(entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        public void OnDestroy(ref SystemState state)
        {

        }
    }
}
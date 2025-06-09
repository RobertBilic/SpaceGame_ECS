using Unity.Burst;
using Unity.Entities;

namespace SpaceGame.Animations.Systems
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    partial struct AutoplayPSSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<AutoplayParticleSystem>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            foreach (var (particleSystem,entity) in SystemAPI.Query<SystemAPI.ManagedAPI.UnityEngineComponent<UnityEngine.ParticleSystem>>()
                .WithAll<AutoplayParticleSystem>()
                .WithNone<ParticleSystemPlaying>()
                .WithEntityAccess())
            {
                particleSystem.Value.Simulate(0.0f, false, true);
                particleSystem.Value.Play();
                ecb.AddComponent<ParticleSystemPlaying>(entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }

        public void OnDestroy(ref SystemState state)
        {

        }
    }
}
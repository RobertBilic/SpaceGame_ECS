using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace SpaceGame.Combat.Components
{
    public struct TurretConstructionRequest : IComponentData
    {
        public Entity RootEntity;
        public FixedString32Bytes Id;
        public float Scale;
        public float3 Position;
        public int Team;
    }
}

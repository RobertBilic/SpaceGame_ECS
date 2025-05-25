using Unity.Entities;
using Unity.Mathematics;

class PlayerSpawnPosition : IComponentData
{
    public float3 Position;
    public float Radius;
}

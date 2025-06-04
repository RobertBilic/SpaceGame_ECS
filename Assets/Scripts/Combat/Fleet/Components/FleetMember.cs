using Unity.Entities;
using Unity.Mathematics;

public struct FleetMember : IComponentData
{
    public Entity FleetReference;
    public float3 LocalOffset;
    public FleetCommand Command;
}

public enum FleetCommand : short
{
    Follow, //Follows the commander, has independant AI
    Intercept, //Intercepts small/medium ships in range
    Bombard, //Attakcs large/capital ships in range
    Defend, //Reacts when commander or that invidual ship is attacked
    Attack //Attacks the target of the commander
}
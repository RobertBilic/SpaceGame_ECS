using Unity.Entities;

public struct GlobalTimeComponent : IComponentData
{
    public double ElapsedTime;
    public double ElapsedTimeScaled;

    public float DeltaTime;
    public float DeltaTimeScaled;

    public long FrameCount;
    public long FrameCountScaled;
}


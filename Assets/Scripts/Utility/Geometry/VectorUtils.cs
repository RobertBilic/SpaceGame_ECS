using Unity.Mathematics;

public static class VectorUtils
{
    public static float3 GetRotatedDirection(float3 dir, float angleDeg)
    {
        float radians = math.radians(angleDeg);
        float cos = math.cos(radians);
        float sin = math.sin(radians);
        return math.normalize(new float3(
            dir.x * cos - dir.y * sin,
            dir.x * sin + dir.y * cos,
            0f
        ));
    }

}

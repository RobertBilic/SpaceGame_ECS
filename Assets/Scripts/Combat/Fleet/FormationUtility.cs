using Unity.Collections;
using Unity.Mathematics;

public static class FormationUtility
{
    public static NativeList<float3> GeneratePackedFormation(NativeList<float> shipRadii, float padding = 1f)
    {
        NativeList<PackedShip> placed = new NativeList<PackedShip>(shipRadii.Length, Allocator.Temp);
        NativeList<float3> result = new NativeList<float3>(shipRadii.Length, Allocator.Temp);

        placed.Add(new PackedShip { Position = float3.zero, Radius = shipRadii[0] });
        result.Add(float3.zero);

        for (int i = 1; i < shipRadii.Length; i++)
        {
            float radius = shipRadii[i];
            bool placedThis = false;

            for (int j = 0; j < placed.Length && !placedThis; j++)
            {
                var existing = placed[j];
                float angleStep = math.radians(20f);

                for (float angle = 0f; angle < math.PI * 2; angle += angleStep)
                {
                    float distance = existing.Radius + radius + padding;
                    float3 candidate = existing.Position + new float3(math.cos(angle), math.sin(angle), 0f) * distance;

                    bool overlaps = false;
                    foreach (var other in placed)
                    {
                        float minDist = radius + other.Radius + padding;
                        if (math.distancesq(candidate, other.Position) < minDist * minDist)
                        {
                            overlaps = true;
                            break;
                        }
                    }

                    if (!overlaps)
                    {
                        placed.Add(new PackedShip { Position = candidate, Radius = radius });
                        result.Add(candidate);
                        placedThis = true;
                        break;
                    }
                }
            }
        }

        placed.Dispose();
        return result;
    }

}

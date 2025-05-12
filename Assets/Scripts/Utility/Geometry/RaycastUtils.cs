using Unity.Mathematics;

public static class RaycastUtils
{
    public static bool RayIntersectsAABB(float3 rayOrigin, float3 rayDir, float3 aabbMin, float3 aabbMax, out float tMin)
    {
        tMin = 0f;
        float tMax = float.MaxValue;

        for (int i = 0; i < 3; i++)
        {
            float rayOrig = rayOrigin[i];
            float rayDirI = rayDir[i];
            float minI = aabbMin[i];
            float maxI = aabbMax[i];

            if (math.abs(rayDirI) < 1e-6f)
            {
                if (rayOrig < minI || rayOrig > maxI)
                    return false;
            }
            else
            {
                float ood = 1.0f / rayDirI;
                float t1 = (minI - rayOrig) * ood;
                float t2 = (maxI - rayOrig) * ood;

                if (t1 > t2) (t1, t2) = (t2, t1);

                tMin = math.max(tMin, t1);
                tMax = math.min(tMax, t2);

                if (tMin > tMax)
                    return false;
            }
        }

        return true;
    }

}

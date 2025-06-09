using Unity.Mathematics;

public static class PhysicsCustom
{
    public static bool CheckOBBOverlap(float3 centerA, float3 halfExtA, quaternion rotA,
        float3 centerB, float3 halfExtB, quaternion rotB)
    {
        float3x3 rotMatA = new float3x3(rotA);
        float3x3 rotMatB = new float3x3(rotB);

        float3 T = math.mul(math.transpose(rotMatA), centerB - centerA); 
        float3x3 R = math.mul(math.transpose(rotMatA), rotMatB);

        float epsilon = 1e-3f; 

        for (int i = 0; i < 3; i++)
        {
            float ra = halfExtA[i];
            float rb = math.abs(R[i][0]) * halfExtB.x + math.abs(R[i][1]) * halfExtB.y + math.abs(R[i][2]) * halfExtB.z;
            if (math.abs(T[i]) > ra + rb + epsilon)
                return false;
        }

        for (int i = 0; i < 3; i++)
        {
            float ra = math.abs(R[0][i]) * halfExtA.x + math.abs(R[1][i]) * halfExtA.y + math.abs(R[2][i]) * halfExtA.z;
            float rb = halfExtB[i];
            float proj = math.abs(math.dot(T, new float3(R[0][i], R[1][i], R[2][i])));
            if (proj > ra + rb + epsilon)
                return false;
        }

        return true; 
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class GizmoUtility
{
    private const float GIZMO_DISK_THICKNESS = 0.01f;

    public static void DrawGizmoDisk(Matrix4x4 localMatrix, Vector3 localOffset, float radius, Color color)
    {
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.color = color;
        Gizmos.matrix = Matrix4x4.TRS(localMatrix.GetPosition(), localMatrix.rotation, new Vector3(1.0f, 1.0f, GIZMO_DISK_THICKNESS));
        Gizmos.DrawWireSphere(localOffset, radius);
        Gizmos.matrix = oldMatrix;
    }
}

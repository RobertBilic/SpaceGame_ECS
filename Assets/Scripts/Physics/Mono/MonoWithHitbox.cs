using System.Collections.Generic;
using UnityEngine;

public class MonoWithHitbox : MonoBehaviour
{
    public List<Hitbox> Hitboxes = new List<Hitbox>();
    public float BoundingRadius;

    void OnDrawGizmosSelected()
    {
        if (Hitboxes == null)
            return;

        Gizmos.color = Color.green;

        foreach (var hitbox in Hitboxes)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(hitbox.LocalCenter, hitbox.HalfExtents * 2f);
        }

        GizmoUtility.DrawGizmoDisk(transform.localToWorldMatrix, Vector3.zero, BoundingRadius, Color.yellow);
    }
}

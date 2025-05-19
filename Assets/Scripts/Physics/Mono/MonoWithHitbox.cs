using System.Collections.Generic;
using UnityEngine;

public class MonoWithHitbox : MonoBehaviour
{
    public List<Hitbox> Hitboxes = new List<Hitbox>();

    void OnDrawGizmosSelected()
    {
        if (Hitboxes == null)
            return;

        Gizmos.color = Color.green;

        foreach (var hitbox in Hitboxes)
        {
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position + transform.rotation * hitbox.LocalCenter, transform.rotation * Quaternion.Euler(hitbox.LocalRotationEuler), Vector3.one);
            Gizmos.matrix = rotationMatrix;
            Gizmos.DrawWireCube(Vector3.zero, hitbox.HalfExtents * 2f);
        }
    }
}

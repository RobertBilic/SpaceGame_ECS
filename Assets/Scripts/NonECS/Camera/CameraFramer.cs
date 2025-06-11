using System.Collections.Generic;
using UnityEngine;

public class CameraFramer : MonoBehaviour
{
    public CameraFollow DefaultCameraBehaviour;
    public Camera targetCamera;
    public float padding = 1.1f;
    public Rect viewportRect = new Rect(0f, 0f, 1f, 1f);

    public void FrameHitboxes(Transform transform, List<Hitbox> shipHitboxes)
    {
        if (targetCamera == null || shipHitboxes.Count == 0)
            return;

        Bounds combined = default;
        bool hasBounds = false;

        foreach (var hb in shipHitboxes)
        {
            var worldBounds = GetWorldBoundsFromHitbox(hb, transform.position, transform.rotation);

            if (!hasBounds)
            {
                combined = worldBounds;
                hasBounds = true;
            }
            else
            {
                combined.Encapsulate(worldBounds);
            }
        }

        if (!hasBounds)
            return;

        Vector3 size = combined.size * padding;

        if (targetCamera.orthographic)
        {
            DefaultCameraBehaviour.Enabled = false;

            float aspect = targetCamera.aspect * (viewportRect.width / viewportRect.height);

            float camSizeY = size.y / (2f * viewportRect.height);
            float camSizeX = (size.x / aspect) / (2f * viewportRect.height);
            targetCamera.orthographicSize = Mathf.Max(camSizeX, camSizeY);

            Vector3 camPos = new Vector3(combined.center.x, combined.center.y, -10.0f);
            targetCamera.transform.position = camPos;
        }
        else
        {
            Debug.LogWarning("Perspective camera not supported.");
        }
    }

    public void UnFrameHitboxes()
    {
        DefaultCameraBehaviour.Enabled = true;
    }

    public static Bounds GetWorldBoundsFromHitbox(Hitbox hitbox, Vector3 worldPos, Quaternion worldRot)
    {
        var trs = Matrix4x4.TRS(worldPos, worldRot * Quaternion.Euler(hitbox.LocalRotationEuler), Vector3.one);

        Vector3[] corners = new Vector3[8];
        int i = 0;
        for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
                for (int z = -1; z <= 1; z += 2)
                {
                    var offset = new Vector3(x, y, z) * 0.5f;
                    var corner = hitbox.LocalCenter + Vector3.Scale(offset, hitbox.HalfExtents * 2);
                    corners[i++] = trs.MultiplyPoint3x4(corner);
                }

        Bounds bounds = new Bounds(corners[0], Vector3.zero);
        for (int j = 1; j < 8; j++)
            bounds.Encapsulate(corners[j]);

        return bounds;
    }
}

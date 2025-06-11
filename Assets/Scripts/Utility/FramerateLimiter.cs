using UnityEngine;

public class FramerateLimiter : MonoBehaviour
{
    public int Framerate;

    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = Framerate;
    }
}

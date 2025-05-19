using UnityEngine;

public class CombatUIController : GenericGameStateUIController<CombatStateUI>
{
    private void Update()
    {
        SetFPS(1.0f / Time.deltaTime);
    }

    private void SetFPS(float fps)
    {
        ui.SetFPS(fps);
    }
}

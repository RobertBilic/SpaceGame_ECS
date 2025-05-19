using SpaceGame.Game.State.Component;
using System.Text;
using TMPro;
using UnityEngine;

public class CombatStateUI : GameStateUI
{
    [SerializeField]
    private TextMeshProUGUI fpsText;

    private StringBuilder sb;
    private void Awake()
    {
        sb = new StringBuilder();
    }

    public override GameState GetRequiredGameState() => GameState.Combat;
    
    public void SetFPS(float fps)
    {
        sb.Clear();

        sb.Append("FPS ");
        sb.Append(fps.ToString("N1"));

        fpsText.text = sb.ToString();
    }

}

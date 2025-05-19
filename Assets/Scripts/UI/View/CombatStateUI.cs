using SpaceGame.Game.State.Component;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatStateUI : GameStateUI
{
    [SerializeField]
    private TextMeshProUGUI fpsText;

    [SerializeField]
    private Button normalSpeedButton;
    [SerializeField]
    private Button doubleSpeedButton;
    [SerializeField]
    private Button tripleSpeedButton;
    [SerializeField]
    private Button pauseButton;

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

    public void SetSpeedChangeAction(OnTimeChangeButtonPressed action)
    {
        pauseButton.onClick.AddListener(() => action(0.0f));
        normalSpeedButton.onClick.AddListener(() => action(1.0f));
        doubleSpeedButton.onClick.AddListener(() => action(2.0f));
        tripleSpeedButton.onClick.AddListener(() => action(3.0f));
    }
}

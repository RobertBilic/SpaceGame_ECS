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

    [SerializeField]
    private Color activeSpeedColor;
    [SerializeField]
    private Color deactivatedSpeedColor;

    private StringBuilder sb;
    private void Awake()
    {
        sb = new StringBuilder();
        ToggleActiveStateForSpeedButtons(normalSpeedButton);
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
        pauseButton.onClick.AddListener(() => {
            action(0.0f);
            ToggleActiveStateForSpeedButtons(pauseButton);
        });
        normalSpeedButton.onClick.AddListener(() => {
            action(1.0f);
            ToggleActiveStateForSpeedButtons(normalSpeedButton);
        });
        doubleSpeedButton.onClick.AddListener(() => {
            action(2.0f);
            ToggleActiveStateForSpeedButtons(doubleSpeedButton);
        });
        tripleSpeedButton.onClick.AddListener(() => {
            action(3.0f);
            ToggleActiveStateForSpeedButtons(tripleSpeedButton);
        });
    }

    private void ToggleActiveStateForSpeedButtons(Button activeButton)
    {
        pauseButton.image.color = activeButton == pauseButton ? activeSpeedColor : deactivatedSpeedColor;
        normalSpeedButton.image.color = activeButton == normalSpeedButton ? activeSpeedColor : deactivatedSpeedColor;
        doubleSpeedButton.image.color = activeButton == doubleSpeedButton ? activeSpeedColor : deactivatedSpeedColor;
        tripleSpeedButton.image.color = activeButton == tripleSpeedButton ? activeSpeedColor : deactivatedSpeedColor;
    }
}

using UnityEngine;
using TMPro;

public class UIManagerNight : MonoBehaviour
{
    public static UIManagerNight Instance { get; private set; }

    [Header("UI Elements")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI phaseText;

    private void Awake()
    {
        Instance = this;
    }

    public void InitUI(int dayCount)
    {
        UpdatePhaseText(dayCount, isDay: false);
        UpdateTimerUI(GameManager.Instance.CFG.dayDuration, GameManager.Instance.CFG.dayDuration);
    }

    public void UpdateTimerUI(float current, float max)
    {
        if (timerText == null) return;
        var minutes = Mathf.FloorToInt(current / 60f);
        var seconds = Mathf.FloorToInt(current % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    public void UpdatePhaseText(int dayCount, bool isDay)
    {
        if (phaseText == null) return;
        phaseText.text = (isDay ? "Day " : "Night ") + dayCount;
    }
}

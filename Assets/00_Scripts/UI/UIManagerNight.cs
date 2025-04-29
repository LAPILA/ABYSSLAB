using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class UIManagerNight : MonoBehaviour
{
    public static UIManagerNight I { get; private set; }

    [FoldoutGroup("Night UI")]

    [FoldoutGroup("Night UI/Texts")]
    [Required]
    [SerializeField] private Text dayText;

    [FoldoutGroup("Night UI/Texts")]
    [Required]
    [SerializeField] private Text clockText;

    [FoldoutGroup("Night UI/Weather")]
    [Required]
    [SerializeField] private Image weatherIcon;

    /* ───── 내부 상태 ───── */
    private Sprite[] _weatherIcons;
    private bool hasAutoPhaseTriggered = false;
    private GameConfig _cfg => GameManager.Instance.CFG;

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
    }

    private void OnDestroy()
    {
        if (GameStateData.I != null)
            GameStateData.I.OnMinuteTick -= RefreshClock;

        if (TimeManager.Instance != null)
            TimeManager.Instance.OnTimeUpdate -= CheckAutoDayTransition;
    }

    public void InitUI(int today)
    {
        _weatherIcons = _cfg.weatherIcons;

        UpdateDay(today);
        UpdateWeather(GameStateData.I.todayWeatherIdx);
        RefreshClock();

        GameStateData.I.OnMinuteTick += RefreshClock;
        TimeManager.Instance.OnTimeUpdate += CheckAutoDayTransition;

        hasAutoPhaseTriggered = false;
    }

    private void UpdateDay(int day) =>
        dayText.text = $"DAY {day:00}";

    private void UpdateWeather(int idx)
    {
        if (_weatherIcons == null || idx < 0 || idx >= _weatherIcons.Length) return;
        weatherIcon.sprite = _weatherIcons[idx];
    }

    private void RefreshClock()
    {
        int minutes = GameStateData.I.currentMinutes;
        int h24 = minutes / 60;
        int m = minutes % 60;
        bool isAM = h24 < 12;
        int h12 = h24 % 12; if (h12 == 0) h12 = 12;
        clockText.text = $"{h12:00}:{m:00} {(isAM ? "AM" : "PM")}";
    }

    private void CheckAutoDayTransition(float secLeft, float total)
    {
        if (hasAutoPhaseTriggered) return;

        if (GameStateData.I.currentMinutes >= 360)
        {
            hasAutoPhaseTriggered = true;
            TimeManager.Instance.ForceEndPhase();
        }
    }
}

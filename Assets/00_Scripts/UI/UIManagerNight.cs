using UnityEngine;
using UnityEngine.UI;
using ALWeapon;
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

        // MEMO :: 웨폰 매니저의 핸들러한테도 구독하는 내용 추가해야함.
        // 다만 접근 방식을 어떻게 할지 정해야할듯? 싱글턴이니까 그냥 접근하게 둬도 되긴 할텐데 레이스 걸리는지에 대해선 확인 해봐야함.

        hasAutoPhaseTriggered = false;
    }

    // 웨폰 UI 분리하여 관리?
    private void UpdateSelectedWeapon(WeaponBase NewWeapon)
    {
        // 아이콘 등.. 업데이트
    }

    private void UpdateWeaponStatus(WeaponBase NewWeapon)
    {
        // 잔탄 등.. 업데이트
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

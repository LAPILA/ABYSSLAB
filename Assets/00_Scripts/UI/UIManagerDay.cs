using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

public class UIManagerDay : MonoBehaviour
{
    /* ───── 싱글톤 ───── */
    public static UIManagerDay I { get; private set; }

    /* ───── UI 슬롯 ───── */
    [Header("Text Slots")]
    [SerializeField] private Text creditText;
    [SerializeField] private Text dayText;
    [SerializeField] private Text clockText;

    [Header("Weather")]
    [SerializeField] private Image weatherIcon;
    private Sprite[] _weatherIcons;

    [Header("Night Mode Prompt")]
    [SerializeField] private GameObject nightPromptUI;    // 'Hold to change phase' 안내창
    [SerializeField] private Scrollbar holdProgressBar;

    /* ───── 내부 상태 ───── */
    private bool nightPromptActive = false;
    private Coroutine holdCoroutine = null;
    private IA_Player _inputActions;
    private GameConfig _cfg => GameManager.Instance.CFG;

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
    }

    private void OnEnable()
    {
        _inputActions = new IA_Player();
        _inputActions.Player.Enable();
        _inputActions.Player.Interact.started += OnInteractStarted;
        _inputActions.Player.Interact.canceled += OnInteractCanceled;
    }

    private void OnDisable()
    {
        _inputActions?.Player.Disable();
    }

    private void OnDestroy()
    {
        if (GameStateData.I != null)
        {
            GameStateData.I.OnCreditChanged -= UpdateCredit;
            GameStateData.I.OnMinuteTick -= RefreshClock;
        }
    }

    /* ---------------------------------------------------------------------- */
    /*  초기 호출 : 낮 씬 로드 직후 GameManager가 호출                         */
    /* ---------------------------------------------------------------------- */
    public void InitUI(int today)
    {
        _weatherIcons = _cfg.weatherIcons;

        UpdateDay(today);
        UpdateCredit(GameStateData.I.Credit);
        UpdateWeather(GameStateData.I.todayWeatherIdx);
        RefreshClock();

        GameStateData.I.OnCreditChanged += UpdateCredit;
        GameStateData.I.OnMinuteTick += RefreshClock;

        nightPromptUI?.SetActive(false);
        holdProgressBar.size = 0f;
    }

    /* ---------------------------------------------------------------------- */
    /*  날짜, 크레딧, 날씨 갱신                                               */
    /* ---------------------------------------------------------------------- */
    public void UpdateDay(int day) =>
        dayText.text = $"DAY {day:00}";

    private void UpdateCredit(int value) =>
        creditText.text = $"{value:n0} ￦";

    private void UpdateWeather(int idx)
    {
        if (_weatherIcons == null || idx < 0 || idx >= _weatherIcons.Length) return;
        weatherIcon.sprite = _weatherIcons[idx];
    }

    /* ---------------------------------------------------------------------- */
    /*  시계 갱신 (GameStateData 기반)                                         */
    /* ---------------------------------------------------------------------- */
    private void RefreshClock()
    {
        int minutes = GameStateData.I.currentMinutes;
        int h24 = minutes / 60;
        int m = minutes % 60;

        bool isAM = h24 < 12;
        int h12 = h24 % 12; if (h12 == 0) h12 = 12;

        clockText.text = $"{h12:00}:{m:00} {(isAM ? "AM" : "PM")}";

        // 🌙 오후 6시(18:00, 1080분) 넘으면 프롬프트 활성화
        if (!nightPromptActive && minutes >= 1080)
        {
            ActivateNightPrompt();
        }
    }

    /* ---------------------------------------------------------------------- */
    /*  밤 시작 안내 프롬프트 활성화                                           */
    /* ---------------------------------------------------------------------- */
    private void ActivateNightPrompt()
    {
        nightPromptActive = true;
        nightPromptUI?.SetActive(true);
        holdProgressBar.size = 0f;
    }

    /* ---------------------------------------------------------------------- */
    /*  InputAction: E 키 누름/뗌                                               */
    /* ---------------------------------------------------------------------- */
    private void OnInteractStarted(InputAction.CallbackContext ctx)
    {
        if (!nightPromptActive) return;
        if (holdCoroutine == null)
            holdCoroutine = StartCoroutine(HoldToStartNight());
    }

    private void OnInteractCanceled(InputAction.CallbackContext ctx)
    {
        if (!nightPromptActive) return;
        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
            holdCoroutine = null;
            holdProgressBar.size = 0f;
        }
    }

    /* ---------------------------------------------------------------------- */
    /*  E 키 홀드 감지 코루틴                                                  */
    /* ---------------------------------------------------------------------- */
    private IEnumerator HoldToStartNight()
    {
        float holdTime = 0f;
        float requiredHoldTime = 2f;

        while (holdTime < requiredHoldTime)
        {
            holdTime += Time.deltaTime;
            holdProgressBar.size = Mathf.Clamp01(holdTime / requiredHoldTime);
            yield return null;
        }

        // 완료: 밤 시작
        nightPromptUI?.SetActive(false);
        TimeManager.Instance.ForceEndPhase();
    }
}

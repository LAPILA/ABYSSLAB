using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;

public class UIManagerDay : MonoBehaviour
{
    public static UIManagerDay I { get; private set; }

    [FoldoutGroup("Main UI")]

    [FoldoutGroup("Main UI/Texts")]
    [Required]
    [SerializeField] private Text creditText;

    [FoldoutGroup("Main UI/Texts")]
    [Required]
    [SerializeField] private Text dayText;

    [FoldoutGroup("Main UI/Texts")]
    [Required]
    [SerializeField] private Text clockText;

    [FoldoutGroup("Main UI/Weather")]
    [Required]
    [SerializeField] private Image weatherIcon;

    [FoldoutGroup("Main UI/Night Prompt")]
    [Required]
    [SerializeField] private GameObject nightPromptUI;

    [FoldoutGroup("Main UI/Night Prompt")]
    [Required]
    [SerializeField] private Scrollbar holdProgressBar;

    /* ───── 내부 상태 ───── */
    private Sprite[] _weatherIcons;
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

    private void UpdateDay(int day) =>
        dayText.text = $"DAY {day:00}";

    private void UpdateCredit(int value) =>
        creditText.text = $"{value:n0} ￦";

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

        if (!nightPromptActive && minutes >= 1080)
        {
            ActivateNightPrompt();
        }
    }

    private void ActivateNightPrompt()
    {
        nightPromptActive = true;
        nightPromptUI?.SetActive(true);
        holdProgressBar.size = 0f;
    }

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

        nightPromptUI?.SetActive(false);
        TimeManager.Instance.ForceEndPhase();
    }
}

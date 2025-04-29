using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using Sirenix.OdinInspector;

[DefaultExecutionOrder(-100)]
public class GameManager : SerializedMonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Title("Core Settings")]
    [BoxGroup("Core")]
    [Required]
    [Tooltip("게임 설정 ScriptableObject")]
    [SerializeField] private GameConfig gameConfig;
    public GameConfig CFG => gameConfig;

    [Title("Fade Settings")]
    [BoxGroup("Fade")]
    [Required]
    [Tooltip("전체 화면 페이드 CanvasGroup")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;

    [Title("Runtime State")]
    [BoxGroup("Runtime")]
    [ReadOnly]
    [ShowInInspector]
    private int _currentDayCount;

    [BoxGroup("Runtime")]
    [ReadOnly]
    [ShowInInspector]
    public GamePhase CurrentPhase { get; private set; } = GamePhase.None;

    [BoxGroup("Runtime")]
    [ReadOnly]
    [ShowInInspector]
    public int CurrentDay => _currentDayCount;

private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadeCanvasGroup != null)
            fadeCanvasGroup.alpha = 0f;
    }

    private void Start()
    {
        // TimeManager 이벤트 구독
        TimeManager.Instance.OnDayEnd += HandleDayEnd;
        TimeManager.Instance.OnNightEnd += HandleNightEnd;

        StartCoroutine(DelayedStart());
    }

    private IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(gameConfig.startDelay);
        ChangePhase(GamePhase.Day);
    }

    /// <summary>
    /// 낮/밤 페이즈 변경
    /// </summary>
    public void ChangePhase(GamePhase nextPhase)
    {
        CurrentPhase = nextPhase;
        GameStateData.I.currentPhase = nextPhase;

        string targetScene = (nextPhase == GamePhase.Day) ? CFG.daySceneName : CFG.nightSceneName;
        StartCoroutine(TransitionScene(targetScene));
    }

    private IEnumerator TransitionScene(string sceneName)
    {
        if (fadeCanvasGroup != null)
            yield return fadeCanvasGroup.DOFade(1f, CFG.fadeDuration).WaitForCompletion();

        yield return SceneManager.LoadSceneAsync(sceneName);

        if (CurrentPhase == GamePhase.Day)
            OnDaySceneLoaded();
        else
            OnNightSceneLoaded();

        if (fadeCanvasGroup != null)
            yield return fadeCanvasGroup.DOFade(0f, CFG.fadeDuration).WaitForCompletion();
    }

    private void OnDaySceneLoaded()
    {
        GameStateData.I.StartNewDay();
        _currentDayCount = GameStateData.I.currentDay;

        UIManagerDay.I?.InitUI(_currentDayCount);

        GameStateData.I.currentMinutes = GameManager.Instance.CFG.dayStartMinutes;

        TimeManager.Instance.StartPhaseTimer(true);
    }


    private void OnNightSceneLoaded()
    {
        GameStateData.I.currentMinutes = CFG.nightStartMinutes;

        UIManagerNight.I?.InitUI(_currentDayCount);
        TimeManager.Instance.StartPhaseTimer(false);
    }

    private void HandleDayEnd() => ChangePhase(GamePhase.Night);
    private void HandleNightEnd() => ChangePhase(GamePhase.Day);

#if UNITY_EDITOR
    [Button(ButtonSizes.Large)]
    [GUIColor(0.9f, 0.8f, 0.4f)]
    [LabelText("낮/밤 전환 (디버그용)")]
    private void ToggleDayNight()
    {
        if (CurrentPhase == GamePhase.Day)
            ChangePhase(GamePhase.Night);
        else if (CurrentPhase == GamePhase.Night)
            ChangePhase(GamePhase.Day);
        else
            Debug.LogWarning("현재 페이즈가 Day/Night가 아닙니다.");
    }
#endif
}

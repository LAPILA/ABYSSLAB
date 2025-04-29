//==============================================================================
// GameManager
// 전역 싱글톤
// 씬 전환(Fade) & 낮/밤 페이즈 토글
// GameStateData 초기화, UIManager 초기화, Score 계산 호출
//==============================================================================

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("■ Core Settings")]
    [SerializeField] private GameConfig gameConfig;
    public GameConfig CFG => gameConfig;

    [Header("■ Fade UI")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;

    private int _currentDayCount;
    public GamePhase CurrentPhase { get; private set; } = GamePhase.None;
    public int CurrentDay => _currentDayCount;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadeCanvasGroup != null) fadeCanvasGroup.alpha = 0f;
    }

    private void Start()
    {
        // TimeManager 이벤트 구독
        TimeManager.Instance.OnDayEnd += HandleDayEnd;
        TimeManager.Instance.OnNightEnd += HandleNightEnd;

        // 첫 씬 진입: Title → Day
        StartCoroutine(DelayedStart());
    }

    private IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(gameConfig.startDelay);
        ChangePhase(GamePhase.Day);
    }

    /// <summary>
    /// 낮/밤 전환 진입점
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
        if (fadeCanvasGroup) yield return fadeCanvasGroup.DOFade(1f, CFG.fadeDuration).WaitForCompletion();
        yield return SceneManager.LoadSceneAsync(sceneName);

        if (CurrentPhase == GamePhase.Day) OnDaySceneLoaded();
        else OnNightSceneLoaded();

        if (fadeCanvasGroup) yield return fadeCanvasGroup.DOFade(0f, CFG.fadeDuration).WaitForCompletion();
    }

    private void OnDaySceneLoaded()
    {
        GameStateData.I.StartNewDay();
        _currentDayCount = GameStateData.I.currentDay;

        UIManagerDay.I?.InitUI(_currentDayCount);

        float duration = GameManager.Instance.CFG.dayStartMinutes;
        TimeManager.Instance.StartPhaseTimer(true, duration);
    }


    private void OnNightSceneLoaded()
    {
        GameStateData.I.currentMinutes = 0;

        UIManagerNight.I?.InitUI(_currentDayCount);

        float duration = GameManager.Instance.CFG.dayEndMinutes;
        TimeManager.Instance.StartPhaseTimer(false, duration);
    }

    private void HandleDayEnd() => ChangePhase(GamePhase.Night);
    private void HandleNightEnd() => ChangePhase(GamePhase.Day);
}

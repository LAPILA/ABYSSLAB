//==============================================================================
//  - 전역 싱글톤
//  - 게임 시작부터 낮/밤 주기 제어
//  - Phase 전환 시 UI 업데이트, 데이터 초기화/증가, 점수 계산 호출
//  - 씬 전환 및 페이드 아웃/인 제어
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

    [Header("Core Settings")]
    [SerializeField] private GameConfig gameConfig;

    [Header("Fade UI")]
    [Tooltip("화면 전체를 덮는 CanvasGroup (알파 0→1 페이드 아웃, 1→0 페이드 인)")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;

    public GamePhase CurrentPhase { get; private set; } = GamePhase.None;
    public GameConfig CFG => gameConfig;

    // 내부 추적용
    private int currentDayCount = 0;

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 초기 페이드 완전 투명
        if (fadeCanvasGroup != null) fadeCanvasGroup.alpha = 0f;
    }

    private void Start()
    {
        // TimeCycleManager 이벤트 구독
        var tcm = TimeCycleManager.Instance;
        tcm.OnDayEnd += OnDayEnd;
        tcm.OnNightEnd += OnNightEnd;

        // 게임 시작 지연 후 첫 낮으로
        StartCoroutine(DelayedStart());
    }

    private IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(gameConfig.startDelay);
        ChangePhase(GamePhase.Day);
    }

    /// <summary>
    /// 페이즈 전환 핵심 메소드
    /// </summary>
    public void ChangePhase(GamePhase nextPhase)
    {
        CurrentPhase = nextPhase;
        GameStateData.Instance.currentPhase = nextPhase;

        switch (nextPhase)
        {
            case GamePhase.Day:
                StartCoroutine(TransitionToPhase(
                    gameConfig.daySceneName,
                    gameConfig.fadeDuration,
                    OnDaySceneLoaded
                ));
                break;

            case GamePhase.Night:
                StartCoroutine(TransitionToPhase(
                    gameConfig.nightSceneName,
                    gameConfig.fadeDuration,
                    OnNightSceneLoaded
                ));
                break;
        }
    }

    /// <summary>
    /// 씬 로드 + 페이드 인/아웃 + 콜백 실행
    /// </summary>
    private IEnumerator TransitionToPhase(string sceneName, float fadeDur, Action onLoaded)
    {
        // 페이드 아웃
        if (fadeCanvasGroup != null)
            yield return fadeCanvasGroup.DOFade(1f, fadeDur).WaitForCompletion();

        // 씬 로드
        yield return SceneManager.LoadSceneAsync(sceneName);

        // 콜백
        onLoaded?.Invoke();

        // 페이드 인
        if (fadeCanvasGroup != null)
            yield return fadeCanvasGroup.DOFade(0f, fadeDur).WaitForCompletion();
    }

    /// <summary>
    /// 낮 씬이 로딩된 직후 처리
    /// </summary>
    private void OnDaySceneLoaded()
    {
        currentDayCount++;
        GameStateData.Instance.StartNewDay();
        GameStateData.Instance.currentDay = currentDayCount;

        if (UIManagerDay.Instance != null)
            UIManagerDay.Instance.InitUI(currentDayCount);

        ScoreManager.Instance.CalculateDayScore();

        StartCoroutine(FadeInAndStartTimer(isDay: true));
    }

    /// <summary>
    /// 밤 씬이 로딩된 직후 처리
    /// </summary>
    private void OnNightSceneLoaded()
    {
        if (UIManagerNight.Instance != null)
            UIManagerNight.Instance.InitUI(currentDayCount);

        StartCoroutine(FadeInAndStartTimer(isDay: false));
    }

    /// <summary>
    /// 낮 타이머 종료 시 호출
    /// </summary>
    private void OnDayEnd()
    {
        ChangePhase(GamePhase.Night);
    }

    /// <summary>
    /// 밤 타이머 종료 시 호출
    /// </summary>
    private void OnNightEnd()
    {
        ChangePhase(GamePhase.Day);
    }

    /// <summary>
    /// 페이드 인 이후 타이머 시작
    /// </summary>
    private IEnumerator FadeInAndStartTimer(bool isDay)
    {
        if (fadeCanvasGroup != null)
            yield return fadeCanvasGroup.DOFade(0f, CFG.fadeDuration).WaitForCompletion();

        float duration = isDay ? CFG.dayDuration : CFG.nightDuration;
        TimeCycleManager.Instance.StartTimer(duration, isDay);
    }

    /// <summary>
    /// 현재 진행 중인 날짜 반환
    /// </summary>
    public int GetCurrentDay() => currentDayCount;
}

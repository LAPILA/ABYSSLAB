using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using Sirenix.OdinInspector;

/// <summary>
/// 게임 전체 메인 흐름 및 페이즈/씬 제어, 페이드 연출 담당 (싱글톤)
/// </summary>
[DefaultExecutionOrder(-100)]
public class GameManager : SerializedMonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // === Config/상태 참조 ===
    [Title("Core Settings")]
    [BoxGroup("Core"), Required, Tooltip("게임 설정 ScriptableObject")]
    [SerializeField] private GameConfig gameConfig;
    public GameConfig CFG => gameConfig;

    [Title("페이드 연출")]
    [BoxGroup("Fade"), Required, Tooltip("전체 화면 페이드 CanvasGroup")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;

    // === 런타임 상태 ===
    [Title("Runtime State")]
    [BoxGroup("Runtime")]
    [ReadOnly, ShowInInspector]
    public GamePhase CurrentPhase { get; private set; } = GamePhase.None;

    [BoxGroup("Runtime")]
    [ReadOnly, ShowInInspector]
    public int CurrentDay => GameStateData.I?.currentDay ?? 1;

    // === 페이즈 변경 이벤트 ===
    public event Action<GamePhase> OnPhaseChanged;

    // ===== 초기화 =====
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
        StartCoroutine(DelayedStart());
    }

    private IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(CFG.startDelay);
        // 시작시 Title 씬/페이즈로 진입
        ChangePhase(GamePhase.Title);
    }

    // ===== 페이즈 전환 =====
    public void ChangePhase(GamePhase nextPhase)
    {
        // 상태 동기화
        CurrentPhase = nextPhase;
        if (GameStateData.I != null)
            GameStateData.I.currentPhase = nextPhase;

        // (디버깅/테스트용 로그)
        Debug.Log($"[GameManager] ChangePhase: {nextPhase}");

        // === 씬 결정 ===
        string targetScene = nextPhase switch
        {
            GamePhase.Day => CFG.daySceneName,
            GamePhase.Night => CFG.nightSceneName,
            GamePhase.Title => CFG.titleSceneName,
            GamePhase.Result => CFG.daySceneName,      // 필요시 별도 씬 지정
            GamePhase.Tutorial => CFG.daySceneName,      // 필요시 별도 씬 지정
            GamePhase.Event => CFG.daySceneName,      // 필요시 별도 씬 지정
            GamePhase.GameOver => CFG.titleSceneName,    // 필요시 GameOver 씬 지정
            _ => CFG.daySceneName,
        };

        // === 씬 전환 ===
        StartCoroutine(TransitionScene(targetScene, nextPhase));

        // === 외부 시스템 이벤트 트리거 ===
        OnPhaseChanged?.Invoke(nextPhase);
    }

    // ===== 씬 전환 및 후처리 Hook =====
    private IEnumerator TransitionScene(string sceneName, GamePhase phase)
    {
        // 1. 페이드아웃
        if (fadeCanvasGroup != null)
            yield return fadeCanvasGroup.DOFade(1f, CFG.fadeDuration).WaitForCompletion();

        // 2. 씬 로드 (이미 로딩중인 경우 방지)
        if (SceneManager.GetActiveScene().name != sceneName)
            yield return SceneManager.LoadSceneAsync(sceneName);

        // 3. 페이즈별 후처리
        switch (phase)
        {
            case GamePhase.Day: OnDaySceneLoaded(); break;
            case GamePhase.Night: OnNightSceneLoaded(); break;
            case GamePhase.Title: OnTitleSceneLoaded(); break;
            case GamePhase.Result: OnResultSceneLoaded(); break;
            case GamePhase.Tutorial: OnTutorialSceneLoaded(); break;
            case GamePhase.Event: OnEventSceneLoaded(); break;
            case GamePhase.GameOver: OnGameOverLoaded(); break;
        }

        // 4. 페이드인
        if (fadeCanvasGroup != null)
            yield return fadeCanvasGroup.DOFade(0f, CFG.fadeDuration).WaitForCompletion();
    }

    // ===== 페이즈별 AfterLoad Hook (각자 연출/세팅 가능) =====
    private void OnDaySceneLoaded()
    {
        // GameStateData.I.StartNewDay(); // 낮 전용 리셋/처리
        Debug.Log("[GameManager] DaySceneLoaded");
    }
    private void OnNightSceneLoaded()
    {
        // GameStateData.I.StartNight(); // 밤 전용 리셋/처리
        Debug.Log("[GameManager] NightSceneLoaded");
    }
    private void OnTitleSceneLoaded()
    {
        Debug.Log("[GameManager] TitleSceneLoaded");
    }
    private void OnResultSceneLoaded()
    {
        Debug.Log("[GameManager] ResultSceneLoaded");
    }
    private void OnTutorialSceneLoaded()
    {
        Debug.Log("[GameManager] TutorialSceneLoaded");
    }
    private void OnEventSceneLoaded()
    {
        Debug.Log("[GameManager] EventSceneLoaded");
    }
    private void OnGameOverLoaded()
    {
        Debug.Log("[GameManager] GameOverLoaded");
    }

    public void GameOver()
    {
        Debug.Log("[GameManager] GAME OVER!");
        ChangePhase(GamePhase.GameOver);
    }

    // ====== 디버그/QA 편의 버튼 ======
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

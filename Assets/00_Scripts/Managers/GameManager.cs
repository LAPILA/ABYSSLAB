using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using Sirenix.OdinInspector;

[DefaultExecutionOrder(-100)]
public class GameManager : SerializedMonoBehaviour
{
    /* ──────────────────────────────────────────────
     * Singleton
     * ────────────────────────────────────────────── */
    public static GameManager Instance { get; private set; }

    /* ──────────────────────────────────────────────
     * Config
     * ────────────────────────────────────────────── */
    [Title("Core Settings")]
    [BoxGroup("Core"), Required][SerializeField] private GameConfig gameConfig;
    public GameConfig CFG => gameConfig;

    [Title("Fade")]
    [BoxGroup("Fade"), Required][SerializeField] private CanvasGroup fadeCanvasGroup;

    /* ──────────────────────────────────────────────
     * Runtime State
     * ────────────────────────────────────────────── */
    [Title("Runtime State")]
    [BoxGroup("Runtime"), ReadOnly, ShowInInspector]
    public GamePhase CurrentPhase { get; private set; } = GamePhase.None;

    [BoxGroup("Runtime"), ReadOnly, ShowInInspector]
    public int CurrentDay => GameStateData.I?.currentDay ?? 1;

    [BoxGroup("Runtime"), ReadOnly, ShowInInspector]
    public DeepSeaWeatherData TodayWeather => GameStateData.I?.TodayWeatherData;

    [Title("Boot")]
    [EnumToggleButtons] public GamePhase initialPhase = GamePhase.Title;

    /* ──────────────────────────────────────────────
     * Events
     * ────────────────────────────────────────────── */
    /// <summary>Prev, Next</summary>
    public event Action<GamePhase, GamePhase> OnPhaseChanged;

    /* ──────────────────────────────────────────────
     * Life-cycle
     * ────────────────────────────────────────────── */
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this; DontDestroyOnLoad(gameObject);

        if (fadeCanvasGroup != null) fadeCanvasGroup.alpha = 0f;
    }

    private void Start() => StartCoroutine(CoDelayedBoot());

    private IEnumerator CoDelayedBoot()
    {
        yield return new WaitForSeconds(CFG.startDelay);
        ChangePhase(initialPhase);   // 첫 진입 Need FIX : 테스트용이라 이후 Title로 픽스
    }

    /* ──────────────────────────────────────────────
     * Public : Phase 변경
     * ────────────────────────────────────────────── */
    public void ChangePhase(GamePhase next)
    {
        if (next == CurrentPhase) return;                          // 동일 페이즈 무시
        var prev = CurrentPhase; CurrentPhase = next;
        if (GameStateData.I) GameStateData.I.currentPhase = next;  // 상태 동기화

        Debug.Log($"[GM] Phase  {prev}  ➜  {next}");

        /* ─ Scene 결정 ─ */
        string targetScene = next switch
        {
            GamePhase.Day => CFG.daySceneName,
            GamePhase.Night => CFG.nightSceneName,
            GamePhase.Title => CFG.titleSceneName,
            GamePhase.Result => CFG.daySceneName,
            GamePhase.Tutorial => CFG.tutorialSceneName,
            GamePhase.Event => CFG.daySceneName,
            GamePhase.GameOver => CFG.titleSceneName,
            _ => CFG.daySceneName,
        };

        StartCoroutine(CoTransitionScene(targetScene, prev, next));
        OnPhaseChanged?.Invoke(prev, next);
    }

    /* ──────────────────────────────────────────────
     *  ▷ Coroutine : Scene Transition
     * ────────────────────────────────────────────── */
    private IEnumerator CoTransitionScene(string sceneName, GamePhase prev, GamePhase next)
    {
        /* 1. Fade Out */
        if (fadeCanvasGroup) yield return fadeCanvasGroup.DOFade(1, CFG.fadeDuration).WaitForCompletion();

        /* 2. Load Scene (필요 시) */
        if (SceneManager.GetActiveScene().name != sceneName)
            yield return SceneManager.LoadSceneAsync(sceneName);

        /* 3. GameStateData 초기화 */
        if (GameStateData.I)
        {
            switch (next)
            {
                case GamePhase.Day: GameStateData.I.StartNewDay(); break;
                case GamePhase.Night: GameStateData.I.StartNight(); break;
                case GamePhase.Title: GameStateData.I.ResetGameState(); break;
            }
        }

        /* 4. Phase별 After-Load Hook */
        switch (next)
        {
            case GamePhase.Day: OnDayLoaded(); break;
            case GamePhase.Night: OnNightLoaded(); break;
            case GamePhase.Title: OnTitleLoaded(); break;
            case GamePhase.Result: OnResultLoaded(); break;
            case GamePhase.Tutorial: OnTutorialLoaded(); break;
            case GamePhase.Event: OnEventLoaded(); break;
            case GamePhase.GameOver: OnGameOverLoaded(); break;
        }

        /* 5. Fade In */
        if (fadeCanvasGroup) yield return fadeCanvasGroup.DOFade(0, CFG.fadeDuration).WaitForCompletion();
    }

    /* ──────────────────────────────────────────────
     *  ▷ Phase-specific Hooks
     * ────────────────────────────────────────────── */
    private void OnDayLoaded() => Debug.Log("[GM] Day Scene Loaded");
    private void OnNightLoaded() => Debug.Log("[GM] Night Scene Loaded");
    private void OnTitleLoaded() => Debug.Log("[GM] Title Scene Loaded");
    private void OnResultLoaded() => Debug.Log("[GM] Result Scene Loaded");
    private void OnTutorialLoaded() => Debug.Log("[GM] Tutorial Scene Loaded");
    private void OnEventLoaded() => Debug.Log("[GM] Event Scene Loaded");
    private void OnGameOverLoaded() => Debug.Log("[GM] GameOver Scene Loaded");

    /* ──────────────────────────────────────────────
     *  ▷ Public : Game Over
     * ────────────────────────────────────────────── */
    public void GameOver()
    {
        Debug.Log("[GM] GAME OVER");
        ChangePhase(GamePhase.GameOver);
    }

#if UNITY_EDITOR
    /* ────────────── Debug Buttons ────────────── */
    [Button(ButtonSizes.Large), GUIColor(0.9f, 0.8f, 0.4f)]
    private void ToggleDayNight()
    {
        if (CurrentPhase == GamePhase.Day) ChangePhase(GamePhase.Night);
        else if (CurrentPhase == GamePhase.Night) ChangePhase(GamePhase.Day);
        else Debug.LogWarning("[GM] 현재 페이즈가 Day/Night 아님");
    }

    [Button(ButtonSizes.Large), GUIColor(0.4f, 0.7f, 1f)]
    private void DebugGameOver() => GameOver();
#endif
}

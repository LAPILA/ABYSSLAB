//==============================================================================
// TimeManager
// - 현실 1초당 게임 내 분 수 누적 (가상 시계)
// - 낮/밤 페이즈 타이머 진행 & UI 갱신 이벤트 (OnTimeUpdate)
// - 페이즈 종료 시 OnDayEnd / OnNightEnd 이벤트 발생
//==============================================================================

using System;
using System.Collections;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;

[DefaultExecutionOrder(-90)]
public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    /* ======================================================================= */
    #region ▶ 런타임 타이머 상태
    /* ======================================================================= */
    [BoxGroup("Runtime Timer")]
    [BoxGroup("Runtime Timer/Phase Timer")]
    [ReadOnly, ShowInInspector]
    private bool _isDayPhase = true;

    [BoxGroup("Runtime Timer/Phase Timer")]
    [ReadOnly, ShowInInspector]
    private bool _isPhaseRunning = true;

    [BoxGroup("Runtime Timer/Clock")]
    [ReadOnly, ShowInInspector]
    private float _accumClock;
    #endregion

    /* ======================================================================= */
    #region ▶ 이벤트
    /* ======================================================================= */

    public event Action<float, float> OnTimeUpdate; // (남은시간, 전체시간)
    public event Action OnDayEnd;
    public event Action OnNightEnd;
    #endregion
    /* ======================================================================= */
    #region ▶ 유니티 생명주기
    /* ======================================================================= */

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 낮/밤 페이즈 타이머 시작
    /// </summary>
    public void StartPhaseTimer(bool isDay)
    {
        _isDayPhase = isDay;
        _isPhaseRunning = true;
    }

    /// <summary>
    /// 낮/밤 시간 체크 (Update에서 매프레임 호출)
    /// </summary>
    private void Update()
    {
        if (!_isPhaseRunning)
            return;

        _accumClock += Time.deltaTime;
        if (_accumClock >= 1f)
        {
            _accumClock -= 1f;

            var current = GameStateData.I.currentMinutes;
            var cfg = GameManager.Instance.CFG;

            if (_isDayPhase)
            {
                if (current >= cfg.dayStartMinutes && current < cfg.dayEndMinutes)
                {
                    GameStateData.I.AdvanceMinutes(cfg.minutesPerRealSecond);
                }
                else
                {
                    _isPhaseRunning = false;
                    Debug.Log("낮 영업 종료 - 대기 상태 진입 (E키 대기)");
                }
            }
            else
            {
                bool isInNightTime =
                    (current >= cfg.nightStartMinutes && current < 1440) ||
                    (current >= 0 && current < cfg.nightEndMinutes);

                if (isInNightTime)
                {
                    GameStateData.I.AdvanceMinutes(cfg.minutesPerRealSecond);
                }
                else
                {
                    _isPhaseRunning = false;
                    Debug.Log("밤 종료 - 자동 낮 전환");
                    GameManager.Instance.ChangePhase(GamePhase.Day);
                }
            }
        }
    }


    #endregion
    /* ======================================================================= */
    #region ▶ 페이즈 종료
    /* ======================================================================= */

    /// <summary>
    /// 강제로 페이즈를 즉시 종료
    /// </summary>
    public void ForceEndPhase()
    {
        StopAllCoroutines();
        StartCoroutine(EndPhaseNow());
    }

    private IEnumerator EndPhaseNow()
    {
        yield return null;
        _isPhaseRunning = false;

        if (_isDayPhase) OnDayEnd?.Invoke();
        else OnNightEnd?.Invoke();
    }

    #endregion

    /* ======================================================================= */
    #region ▶ DEBUG
    /* ======================================================================= */
    [Button("게임 시간 +60분 (디버그용)")]
    private void Debug_Add10Minutes()
    {
        GameStateData.I.AdvanceMinutes(60);
        Debug.Log($"[디버그] 게임 시간 +60분 → 현재: {GameStateData.I.currentMinutes}분");
    }
    /* ======================================================================= */
    #endregion
}

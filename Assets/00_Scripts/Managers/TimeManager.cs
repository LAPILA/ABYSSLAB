//==============================================================================
// TimeManager
//  현실 1초당 게임 내 분 수 누적 (가상 시계)
//  낮/밤 페이즈 타이머 진행 & UI 갱신 이벤트(OnTimeUpdate)
//  페이즈 종료 시 OnDayEnd / OnNightEnd 이벤트 발생
//==============================================================================

using System;
using System.Collections;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    // 페이즈 타이머
    private float _phaseTimer;
    private float _phaseDuration;
    private bool _isDayPhase;
    private bool _isPhaseRunning;

    // 가상 시계 누적
    private float _accumClock;

    /// <summary>1분 단위 흐름을 UI에 알릴 때</summary>
    public event Action<float, float> OnTimeUpdate;
    /// <summary>낮 페이즈가 끝났을 때</summary>
    public event Action OnDayEnd;
    /// <summary>밤 페이즈가 끝났을 때</summary>
    public event Action OnNightEnd;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        // 1) 가상 시계: 매 1초마다 minutesPerRealSecond 만큼 GameStateData에 알림
        _accumClock += Time.deltaTime;
        if (_accumClock >= 1f)
        {
            GameStateData.I.AdvanceMinutes(GameManager.Instance.CFG.minutesPerRealSecond);
            _accumClock -= 1f;
        }
    }

    /// <summary>
    /// 낮/밤 페이즈 타이머 시작
    /// </summary>
    /// <param name="isDay">true=낮, false=밤</param>
    /// <param name="duration">초 단위 지속시간</param>
    public void StartPhaseTimer(bool isDay, float duration)
    {
        _isDayPhase = isDay;
        _phaseDuration = duration;
        _phaseTimer = duration;
        _isPhaseRunning = true;

        StartCoroutine(PhaseTimerCoroutine());
    }

    private IEnumerator PhaseTimerCoroutine()
    {
        while (_isPhaseRunning && _phaseTimer > 0f)
        {
            _phaseTimer -= Time.deltaTime;

            OnTimeUpdate?.Invoke(_phaseTimer, _phaseDuration);

            yield return null;
        }

        _isPhaseRunning = false;

        if (_isDayPhase) OnDayEnd?.Invoke();
        else OnNightEnd?.Invoke();
    }

    /// <summary>
    /// 중간에 즉시 페이즈를 끝내고 싶을 때 (디버그 / 버튼 등)
    /// </summary>
    public void ForceEndPhase()
    {
        _phaseTimer = 0f;
        StopAllCoroutines(); // 혹시 이전 타이머 루프가 남아있다면 정지
        StartCoroutine(EndPhaseNow());
    }

    private IEnumerator EndPhaseNow()
    {
        yield return null; // 다음 프레임까지 기다림
        _isPhaseRunning = false;

        if (_isDayPhase) OnDayEnd?.Invoke();
        else OnNightEnd?.Invoke();
    }
}

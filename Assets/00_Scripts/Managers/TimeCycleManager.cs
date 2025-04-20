//==============================================================================
// TimeCycleManager
// - 낮/밤 타이머의 흐름을 제어
// - 매 프레임 타이머를 갱신하며 UIManager를 통해 시간 표시
// - 타이머 종료 시 GameManager에 낮/밤 전환 알림
//==============================================================================

using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class TimeCycleManager : MonoBehaviour
{
    public static TimeCycleManager Instance { get; private set; }

    /// <summary>낮 종료 시 호출</summary>
    public event Action OnDayEnd;

    /// <summary>밤 종료 시 호출</summary>
    public event Action OnNightEnd;

    private float _timer;        // 남은 시간 (초)
    private float _duration;     // 전체 타이머 시간
    private bool _isRunning;     // 타이머 실행 여부

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    /// <summary>
    /// 타이머 시작
    /// </summary>
    /// <param name="duration">타이머 지속 시간</param>
    /// <param name="isDay">낮 타이머 여부</param>
    public void StartTimer(float duration, bool isDay)
    {
        _duration = duration;
        _timer = duration;
        _isRunning = true;

        StartCoroutine(TickTimer(isDay));
    }

    /// <summary>
    /// 타이머 진행 코루틴 (프레임별로 시간 감소 및 UI 갱신)
    /// </summary>
    private IEnumerator TickTimer(bool isDay)
    {
        string scene = SceneManager.GetActiveScene().name;
        bool isDayScene = scene == GameManager.Instance.CFG.daySceneName;

        while (_timer > 0f)
        {
            _timer -= Time.deltaTime;

            // Clamp: 음수 방지
            float displayTime = Mathf.Max(_timer, 0f);

            // UI 업데이트
            if (isDayScene)
                UIManagerDay.Instance?.UpdateTimerUI(displayTime, _duration);
            else
                UIManagerNight.Instance?.UpdateTimerUI(displayTime, _duration);

            yield return null;
        }

        // 타이머 종료 확정
        _isRunning = false;

        // 종료 시점 0초 UI 갱신 강제
        if (isDayScene)
            UIManagerDay.Instance?.UpdateTimerUI(0f, _duration);
        else
            UIManagerNight.Instance?.UpdateTimerUI(0f, _duration);

        // 낮/밤 종료 콜백
        if (isDay)
            OnDayEnd?.Invoke();
        else
            OnNightEnd?.Invoke();
    }

    /// <summary>남은 시간 조회 (초)</summary>
    public float GetTimeLeft() => _timer;

    /// <summary>타이머 동작 여부</summary>
    public bool IsRunning() => _isRunning;
}

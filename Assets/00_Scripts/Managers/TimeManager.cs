//==============================================================================
// TimeManager
//  ���� 1�ʴ� ���� �� �� �� ���� (���� �ð�)
//  ��/�� ������ Ÿ�̸� ���� & UI ���� �̺�Ʈ(OnTimeUpdate)
//  ������ ���� �� OnDayEnd / OnNightEnd �̺�Ʈ �߻�
//==============================================================================

using System;
using System.Collections;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    // ������ Ÿ�̸�
    private float _phaseTimer;
    private float _phaseDuration;
    private bool _isDayPhase;
    private bool _isPhaseRunning;

    // ���� �ð� ����
    private float _accumClock;

    /// <summary>1�� ���� �帧�� UI�� �˸� ��</summary>
    public event Action<float, float> OnTimeUpdate;
    /// <summary>�� ����� ������ ��</summary>
    public event Action OnDayEnd;
    /// <summary>�� ����� ������ ��</summary>
    public event Action OnNightEnd;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        // 1) ���� �ð�: �� 1�ʸ��� minutesPerRealSecond ��ŭ GameStateData�� �˸�
        _accumClock += Time.deltaTime;
        if (_accumClock >= 1f)
        {
            GameStateData.I.AdvanceMinutes(GameManager.Instance.CFG.minutesPerRealSecond);
            _accumClock -= 1f;
        }
    }

    /// <summary>
    /// ��/�� ������ Ÿ�̸� ����
    /// </summary>
    /// <param name="isDay">true=��, false=��</param>
    /// <param name="duration">�� ���� ���ӽð�</param>
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
    /// �߰��� ��� ����� ������ ���� �� (����� / ��ư ��)
    /// </summary>
    public void ForceEndPhase()
    {
        _phaseTimer = 0f;
        StopAllCoroutines(); // Ȥ�� ���� Ÿ�̸� ������ �����ִٸ� ����
        StartCoroutine(EndPhaseNow());
    }

    private IEnumerator EndPhaseNow()
    {
        yield return null; // ���� �����ӱ��� ��ٸ�
        _isPhaseRunning = false;

        if (_isDayPhase) OnDayEnd?.Invoke();
        else OnNightEnd?.Invoke();
    }
}

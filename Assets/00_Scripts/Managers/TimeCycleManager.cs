//==============================================================================
// TimeCycleManager
// - ��/�� Ÿ�̸��� �帧�� ����
// - �� ������ Ÿ�̸Ӹ� �����ϸ� UIManager�� ���� �ð� ǥ��
// - Ÿ�̸� ���� �� GameManager�� ��/�� ��ȯ �˸�
//==============================================================================

using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class TimeCycleManager : MonoBehaviour
{
    public static TimeCycleManager Instance { get; private set; }

    /// <summary>�� ���� �� ȣ��</summary>
    public event Action OnDayEnd;

    /// <summary>�� ���� �� ȣ��</summary>
    public event Action OnNightEnd;

    private float _timer;        // ���� �ð� (��)
    private float _duration;     // ��ü Ÿ�̸� �ð�
    private bool _isRunning;     // Ÿ�̸� ���� ����

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
    /// Ÿ�̸� ����
    /// </summary>
    /// <param name="duration">Ÿ�̸� ���� �ð�</param>
    /// <param name="isDay">�� Ÿ�̸� ����</param>
    public void StartTimer(float duration, bool isDay)
    {
        _duration = duration;
        _timer = duration;
        _isRunning = true;

        StartCoroutine(TickTimer(isDay));
    }

    /// <summary>
    /// Ÿ�̸� ���� �ڷ�ƾ (�����Ӻ��� �ð� ���� �� UI ����)
    /// </summary>
    private IEnumerator TickTimer(bool isDay)
    {
        string scene = SceneManager.GetActiveScene().name;
        bool isDayScene = scene == GameManager.Instance.CFG.daySceneName;

        while (_timer > 0f)
        {
            _timer -= Time.deltaTime;

            // Clamp: ���� ����
            float displayTime = Mathf.Max(_timer, 0f);

            // UI ������Ʈ
            if (isDayScene)
                UIManagerDay.Instance?.UpdateTimerUI(displayTime, _duration);
            else
                UIManagerNight.Instance?.UpdateTimerUI(displayTime, _duration);

            yield return null;
        }

        // Ÿ�̸� ���� Ȯ��
        _isRunning = false;

        // ���� ���� 0�� UI ���� ����
        if (isDayScene)
            UIManagerDay.Instance?.UpdateTimerUI(0f, _duration);
        else
            UIManagerNight.Instance?.UpdateTimerUI(0f, _duration);

        // ��/�� ���� �ݹ�
        if (isDay)
            OnDayEnd?.Invoke();
        else
            OnNightEnd?.Invoke();
    }

    /// <summary>���� �ð� ��ȸ (��)</summary>
    public float GetTimeLeft() => _timer;

    /// <summary>Ÿ�̸� ���� ����</summary>
    public bool IsRunning() => _isRunning;
}

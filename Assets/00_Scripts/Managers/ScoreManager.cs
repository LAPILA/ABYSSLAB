//==============================================================================
//  ScoreManager
//  ��� ���� ����� �Ѱ����� ���� (�� ��������� �г�Ƽ�����ʽ� ��)
//==============================================================================

using UnityEngine;
using System;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager I { get; private set; }

    /* ���������� ����ġ & ���ʽ� ���������� */
    [Header("���� ����ġ")]
    [Tooltip("ȹ�� ũ���� 1�� ������")]
    [Min(0)] public float weightCredit = 0.5f;
    [Tooltip("�α⵵ 1�� ������")]
    [Min(0)] public float weightPopularity = 1.0f;
    [Tooltip("���� 1�� ������")]
    [Min(0)] public float weightReputation = 2.0f;
    [Header("���� ���� ���ʽ�")]
    public int bonusPerDaySurvived = 50;

    /* ���������� �ݹ� �̺�Ʈ ���������� */
    /// <summary>�Ϸ� ���� ����� ������ �� (���� DayScore ����)</summary>
    public Action<int> OnDayScoreCalculated;

    private void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    /* ====================================================================== */
    /*  �� ������ ���� �� ���� ���                                            */
    /* ====================================================================== */
    public void CalculateDayScore()
    {
        GameStateData st = GameStateData.I;

        /* ���� ���� �׸� ����ġ ���� ���� */
        float scoreCredit = st.Credit * weightCredit;
        float scorePopularity = st.Popularity * weightPopularity;
        float scoreReputation = st.Reputation * weightReputation;

        /* ���� ���� ���� ���ʽ� ���� */
        int survivedBonus = st.currentDay * bonusPerDaySurvived;

        /* ���� ���� ���� */
        int dayScore = Mathf.RoundToInt(
              scoreCredit
            + scorePopularity
            + scoreReputation
            + survivedBonus
        );

        /* ���� GameStateData �ݿ� ���� */
        st.totalScore += dayScore;
        st.successCount++;

        /* ���� UI / �α׸� ���� �ݹ� ���� */
        OnDayScoreCalculated?.Invoke(dayScore);

        Debug.Log($"[Score] Day{st.currentDay}  ��  +{dayScore} pts (Total {st.totalScore})");
    }

    /* ====================================================================== */
    /*  ����(�� �����Ż�� �ټ� ��) �г�Ƽ                                     */
    /* ====================================================================== */
    [Header("�г�Ƽ ����")]
    public int flatPenaltyScore = 100;
    public int penaltyReputation = 5;

    public void ApplyFailurePenalty(string reason = "")
    {
        GameStateData st = GameStateData.I;

        st.totalScore = Mathf.Max(0, st.totalScore - flatPenaltyScore);
        st.failureCount++;
        st.AddReputation(-penaltyReputation);

        Debug.LogWarning($"[Score] Failure Penalty  -{flatPenaltyScore} pts,  -{penaltyReputation} REP  ({reason})");
    }
}

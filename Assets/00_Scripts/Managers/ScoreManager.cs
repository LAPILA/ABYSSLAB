//==============================================================================
// ScoreManager
// - ��� ���� ����� �ϰ� ����
// - �� ��� ���, ���� �� �г�Ƽ �ο�
//==============================================================================

using UnityEngine;
using System;
using Sirenix.OdinInspector;

[DefaultExecutionOrder(-80)]
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager I { get; private set; }

    /* ======================================================================= */
    #region �� ���� ����ġ
    /* ======================================================================= */

    [TitleGroup("Score Settings")]
    [BoxGroup("Score Settings/Weights")]
    [Tooltip("ȹ�� ũ���� 1�� ������")]
    [MinValue(0)] public float weightCredit = 0.5f;

    [BoxGroup("Score Settings/Weights")]
    [Tooltip("�α⵵ 1�� ������")]
    [MinValue(0)] public float weightPopularity = 1.0f;

    [BoxGroup("Score Settings/Weights")]
    [Tooltip("���� 1�� ������")]
    [MinValue(0)] public float weightReputation = 2.0f;

    [BoxGroup("Score Settings/Bonus")]
    [Tooltip("�Ϸ� ������ �߰� ���ʽ�")]
    public int bonusPerDaySurvived = 50;
    #endregion
    /* ======================================================================= */
    #region �� �г�Ƽ ����
    /* ======================================================================= */

    [TitleGroup("Penalty Settings")]
    [Tooltip("���� �� ���� ������")]
    public int flatPenaltyScore = 100;

    [Tooltip("���� �� ���� ������")]
    public int penaltyReputation = 5;
    #endregion
    /* ======================================================================= */
    #region �� �ݹ� �̺�Ʈ
    /* ======================================================================= */

    public event Action<int> OnDayScoreCalculated;
    #endregion
    /* ======================================================================= */
    #region �� ����Ƽ �����ֱ�
    /* ======================================================================= */

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion
    /* ======================================================================= */
    #region �� �� ��� ���� ���
    /* ======================================================================= */

    /// <summary>
    /// �Ϸ簡 ���� �� ���� ���
    /// </summary>
    public void CalculateDayScore()
    {
        var st = GameStateData.I;

        float scoreCredit = st.Credit * weightCredit;
        float scorePopularity = st.Popularity * weightPopularity;
        float scoreReputation = st.Reputation * weightReputation;
        int survivedBonus = st.currentDay * bonusPerDaySurvived;

        int dayScore = Mathf.RoundToInt(
            scoreCredit + scorePopularity + scoreReputation + survivedBonus
        );

        st.totalScore += dayScore;
        st.successCount++;

        OnDayScoreCalculated?.Invoke(dayScore);

        Debug.Log($"[Score] Day {st.currentDay} �� +{dayScore} pts (Total {st.totalScore})");
    }

    #endregion

    /* ======================================================================= */
    #region �� ���� �� �г�Ƽ ����
    /* ======================================================================= */

    /// <summary>
    /// ���� �� �г�Ƽ ����
    /// </summary>
    public void ApplyFailurePenalty(string reason = "")
    {
        var st = GameStateData.I;

        st.totalScore = Mathf.Max(0, st.totalScore - flatPenaltyScore);
        st.failureCount++;
        st.AddReputation(-penaltyReputation);

        Debug.LogWarning($"[Score] Failure Penalty  -{flatPenaltyScore} pts,  -{penaltyReputation} REP ({reason})");
    }

    #endregion
}
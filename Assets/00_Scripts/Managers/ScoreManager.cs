//==============================================================================
// ScoreManager
// - 모든 점수 산식을 일괄 관리
// - 낮 결과 계산, 실패 시 패널티 부여
//==============================================================================

using UnityEngine;
using System;
using Sirenix.OdinInspector;

[DefaultExecutionOrder(-80)]
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager I { get; private set; }

    /* ======================================================================= */
    #region ▶ 점수 가중치
    /* ======================================================================= */

    [TitleGroup("Score Settings")]
    [BoxGroup("Score Settings/Weights")]
    [Tooltip("획득 크레딧 1당 가산점")]
    [MinValue(0)] public float weightCredit = 0.5f;

    [BoxGroup("Score Settings/Weights")]
    [Tooltip("인기도 1당 가산점")]
    [MinValue(0)] public float weightPopularity = 1.0f;

    [BoxGroup("Score Settings/Weights")]
    [Tooltip("평판 1당 가산점")]
    [MinValue(0)] public float weightReputation = 2.0f;

    [BoxGroup("Score Settings/Bonus")]
    [Tooltip("하루 생존당 추가 보너스")]
    public int bonusPerDaySurvived = 50;
    #endregion
    /* ======================================================================= */
    #region ▶ 패널티 설정
    /* ======================================================================= */

    [TitleGroup("Penalty Settings")]
    [Tooltip("실패 시 점수 차감량")]
    public int flatPenaltyScore = 100;

    [Tooltip("실패 시 평판 차감량")]
    public int penaltyReputation = 5;
    #endregion
    /* ======================================================================= */
    #region ▶ 콜백 이벤트
    /* ======================================================================= */

    public event Action<int> OnDayScoreCalculated;
    #endregion
    /* ======================================================================= */
    #region ▶ 유니티 생명주기
    /* ======================================================================= */

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion
    /* ======================================================================= */
    #region ▶ 낮 결과 점수 계산
    /* ======================================================================= */

    /// <summary>
    /// 하루가 끝날 때 점수 계산
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

        Debug.Log($"[Score] Day {st.currentDay} ▶ +{dayScore} pts (Total {st.totalScore})");
    }

    #endregion

    /* ======================================================================= */
    #region ▶ 실패 시 패널티 적용
    /* ======================================================================= */

    /// <summary>
    /// 실패 시 패널티 적용
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
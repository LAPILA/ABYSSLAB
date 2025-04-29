//==============================================================================
//  ScoreManager
//  모든 점수 산식을 한곳에서 관리 (낮 결과·실패 패널티·보너스 등)
//==============================================================================

using UnityEngine;
using System;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager I { get; private set; }

    /* ───── 가중치 & 보너스 ───── */
    [Header("점수 가중치")]
    [Tooltip("획득 크레딧 1당 가산점")]
    [Min(0)] public float weightCredit = 0.5f;
    [Tooltip("인기도 1당 가산점")]
    [Min(0)] public float weightPopularity = 1.0f;
    [Tooltip("평판 1당 가산점")]
    [Min(0)] public float weightReputation = 2.0f;
    [Header("일차 생존 보너스")]
    public int bonusPerDaySurvived = 50;

    /* ───── 콜백 이벤트 ───── */
    /// <summary>하루 점수 계산이 끝났을 때 (계산된 DayScore 전달)</summary>
    public Action<int> OnDayScoreCalculated;

    private void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    /* ====================================================================== */
    /*  낮 페이즈 종료 → 점수 계산                                            */
    /* ====================================================================== */
    public void CalculateDayScore()
    {
        GameStateData st = GameStateData.I;

        /* ── 개별 항목 가중치 적용 ── */
        float scoreCredit = st.Credit * weightCredit;
        float scorePopularity = st.Popularity * weightPopularity;
        float scoreReputation = st.Reputation * weightReputation;

        /* ── 생존 일차 보너스 ── */
        int survivedBonus = st.currentDay * bonusPerDaySurvived;

        /* ── 종합 ── */
        int dayScore = Mathf.RoundToInt(
              scoreCredit
            + scorePopularity
            + scoreReputation
            + survivedBonus
        );

        /* ── GameStateData 반영 ── */
        st.totalScore += dayScore;
        st.successCount++;

        /* ── UI / 로그를 위한 콜백 ── */
        OnDayScoreCalculated?.Invoke(dayScore);

        Debug.Log($"[Score] Day{st.currentDay}  ▶  +{dayScore} pts (Total {st.totalScore})");
    }

    /* ====================================================================== */
    /*  실패(밤 사망·탈출 다수 등) 패널티                                     */
    /* ====================================================================== */
    [Header("패널티 설정")]
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

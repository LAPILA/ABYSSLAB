using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("점수 가중치 설정")]
    public float creditWeight = 1f;
    public float popularityWeight = 2f;
    public float reputationWeight = 3f;
    public int bonusPerDaySurvived = 50;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 낮이 끝날 때 호출해서 점수 계산 → GameStateData에 반영
    /// </summary>
    public void CalculateDayScore()
    {
        var state = GameStateData.Instance;
        float scoreFromCredit = state.credit * creditWeight;
        float scoreFromPopularity = state.popularity * popularityWeight;
        float scoreFromReputation = state.reputation * reputationWeight;
        int dayBonus = state.currentDay * bonusPerDaySurvived;

        int total = Mathf.RoundToInt(scoreFromCredit
                                   + scoreFromPopularity
                                   + scoreFromReputation
                                   + dayBonus);

        state.totalScore += total;
        state.successCount++;
    }

    /// <summary>
    /// 실패 시 페널티를 부여할 때 호출
    /// </summary>
    public void ApplyFailurePenalty()
    {
        var state = GameStateData.Instance;
        state.failureCount++;
        state.totalScore -= 100;          // 고정 페널티
        state.reputation -= 5f;          // 평판 감소
    }
}

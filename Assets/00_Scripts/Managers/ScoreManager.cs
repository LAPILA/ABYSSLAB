using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("���� ����ġ ����")]
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
    /// ���� ���� �� ȣ���ؼ� ���� ��� �� GameStateData�� �ݿ�
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
    /// ���� �� ���Ƽ�� �ο��� �� ȣ��
    /// </summary>
    public void ApplyFailurePenalty()
    {
        var state = GameStateData.Instance;
        state.failureCount++;
        state.totalScore -= 100;          // ���� ���Ƽ
        state.reputation -= 5f;          // ���� ����
    }
}

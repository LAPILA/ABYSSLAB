//==============================================================================
// GameStateData
// 런타임 진행 현황을 단일 싱글톤으로 관리
// 시계/기후/자원/감염/크리처/점수 전부 포함
//==============================================================================

using System;
using System.Collections.Generic;
using UnityEngine;

public class GameStateData : MonoBehaviour
{
    public static GameStateData I { get; private set; }

    /* ───────────── 진행 & 시계 ───────────── */
    public int currentDay = 0;
    public GamePhase currentPhase = GamePhase.None;

    public int currentMinutes = 360;     // 6:00 AM (0~1439)
    public int todayWeatherIdx = 0;       // 0:Clear
    public Action OnMinuteTick;               // HUD 시계 갱신용

    /* ───────────── 자원 ───────────── */
    public int Credit { get; private set; }
    public int Popularity { get; private set; }
    public int Reputation { get; private set; }

    public Action<int> OnCreditChanged;
    public Action<int> OnPopularityChanged;
    public Action<int> OnReputationChanged;

    /* ───────────── 감염 ───────────── */
    public int infectionStage = 0;
    public int nextInfectionUpgradeDay = 14;   // N일 후 +1단계

    /* ───────────── 크리쳐 ───────────── */
    public readonly List<string> ownedCreatures = new();
    public readonly List<string> exhibitedToday = new();

    /* ───────────── 성과 & 점수 ───────────── */
    public int totalScore { get; set; }
    public int successCount { get; set; }
    public int failureCount { get; set; }

    /* ======================================================================= */
    #region ▶ 초기화 & 리셋
    /* ======================================================================= */

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this; DontDestroyOnLoad(gameObject);

        ResetGameState();
    }

    public void ResetGameState()
    {
        var cfg = GameManager.Instance?.CFG;
        if (cfg == null)
        {
            Debug.LogError("[GameStateData] GameManager.Instance.CFG 가 null 입니다!");
            return;
        }
        currentDay = 0;
        currentPhase = GamePhase.Title;
        currentMinutes = 360;
        if (cfg == null)
        {
            Debug.LogError("<GameStateData> GameConfig.asset 을 Resources 폴더에서 찾을 수 없습니다!");
        }
        Credit = cfg.initialCredit;
        Popularity = cfg.initialPopularity;
        Reputation = cfg.initialReputation;

        infectionStage = cfg.startInfectionStage;
        nextInfectionUpgradeDay = cfg.infectionStagePeriodDays;

        ownedCreatures.Clear();
        exhibitedToday.Clear();

        totalScore = 0;
        successCount = 0;
        failureCount = 0;
    }
    #endregion

    /* ======================================================================= */
    #region ▶ 시간‧기후
    /* ======================================================================= */
    public void AdvanceMinutes(int min)
    {
        currentMinutes = (currentMinutes + min) % 1440;
        OnMinuteTick?.Invoke();
    }
    public void RollNewWeather(int idx) => todayWeatherIdx = idx;
    #endregion

    /* ======================================================================= */
    #region ▶ 자원 변경
    /* ======================================================================= */
    public void AddCredit(int amount)
    {
        Credit += amount;
        OnCreditChanged?.Invoke(Credit);
    }
    public void AddPopularity(int delta)
    {
        Popularity += delta;
        OnPopularityChanged?.Invoke(Popularity);
    }
    public void AddReputation(int delta)
    {
        Reputation += delta;
        OnReputationChanged?.Invoke(Reputation);
    }
    #endregion

    /* ======================================================================= */
    #region ▶ 크리쳐 관리
    /* ======================================================================= */
    public void AddOwnedCreature(string id)
    {
        if (!ownedCreatures.Contains(id))
            ownedCreatures.Add(id);
    }
    public void AddExhibitedCreature(string id)
    {
        if (!exhibitedToday.Contains(id))
            exhibitedToday.Add(id);
    }
    #endregion

    /* ======================================================================= */
    #region ▶ 낮/밤 전환 & 감염 진행
    /* ======================================================================= */
    public void StartNewDay()
    {
        currentDay++;
        currentPhase = GamePhase.Day;
        exhibitedToday.Clear();

        /* 감염 단계 체크 */
        if (currentDay >= nextInfectionUpgradeDay)
        {
            infectionStage = Mathf.Min(
                infectionStage + 1,
                Resources.Load<GameConfig>("GameConfig").maxInfectionStage
            );
            nextInfectionUpgradeDay +=
                Resources.Load<GameConfig>("GameConfig").infectionStagePeriodDays;
        }
    }
    #endregion

    /* ======================================================================= */
    #region ▶ 성과 집계 (낮 결과·실패)
    /* ======================================================================= */
    public void ApplyDayResults(int earnCredit, int earnPopularity,
                                int deltaReputation, int earnedScore)
    {
        AddCredit(earnCredit);
        AddPopularity(earnPopularity);
        AddReputation(deltaReputation);

        totalScore += earnedScore;
        successCount += 1;
    }

    public void RegisterFailure(int penaltyScore = 0, int penaltyRep = 5)
    {
        totalScore = Mathf.Max(0, totalScore - penaltyScore);
        Reputation -= penaltyRep;
        failureCount += 1;
        OnReputationChanged?.Invoke(Reputation);
    }
    #endregion
}

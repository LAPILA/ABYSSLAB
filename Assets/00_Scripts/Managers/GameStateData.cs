//==============================================================================
// GameStateData
// - 런타임 전체 게임 상태를 싱글톤으로 관리
// - 시계/기후/자원/감염/크리처/점수 시스템 포함
//==============================================================================

using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-90)]
public class GameStateData : SerializedMonoBehaviour
{
    public static GameStateData I { get; private set; }

    /* ======================================================================= */
    #region ▶ 진행 & 시계
    /* ======================================================================= */

    [TitleGroup("Progress & Clock")]
    [PropertyOrder(0)]
    [ReadOnly, ShowInInspector]
    public int currentDay = 0;

    [PropertyOrder(1)]
    [ReadOnly, ShowInInspector]
    public GamePhase currentPhase = GamePhase.None;

    [PropertyOrder(2)]
    [ReadOnly, ShowInInspector]
    public int currentMinutes = 540;

    [PropertyOrder(3)]
    [ReadOnly, ShowInInspector]
    public int todayWeatherIdx = 0;

    public event Action OnMinuteTick;

    #endregion

    /* ======================================================================= */
    #region ▶ 자원
    /* ======================================================================= */

    [TitleGroup("Resources")]
    [ReadOnly, ShowInInspector]
    public int Credit { get; private set; }

    [ReadOnly, ShowInInspector]
    public int Popularity { get; private set; }

    [ReadOnly, ShowInInspector]
    public int Reputation { get; private set; }

    public event Action<int> OnCreditChanged;
    public event Action<int> OnPopularityChanged;
    public event Action<int> OnReputationChanged;

    #endregion

    /* ======================================================================= */
    #region ▶ 감염
    /* ======================================================================= */

    [TitleGroup("Infection")]
    [ReadOnly, ShowInInspector]
    public int infectionStage = 0;

    [ReadOnly, ShowInInspector]
    public int nextInfectionUpgradeDay = 14;

    #endregion

    /* ======================================================================= */
    #region ▶ 크리처
    /* ======================================================================= */

    [TitleGroup("Creatures")]
    [ShowInInspector]
    public readonly List<string> ownedCreatures = new();

    [ShowInInspector]
    public readonly List<string> exhibitedToday = new();

    #endregion

    /* ======================================================================= */
    #region ▶ 성과
    /* ======================================================================= */

    [TitleGroup("Score")]
    [ReadOnly, ShowInInspector]
    public int totalScore { get; set; }

    [ReadOnly, ShowInInspector]
    public int successCount { get; set; }

    [ReadOnly, ShowInInspector]
    public int failureCount { get; set; }

    #endregion

    /* ======================================================================= */
    #region ▶ 초기화 & 리셋
    /* ======================================================================= */

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

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
        currentMinutes = 540;

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
    #region ▶ 시간·기후 관련
    /* ======================================================================= */

    public void AdvanceMinutes(int min)
    {
        currentMinutes = (currentMinutes + min) % 1440;
        OnMinuteTick?.Invoke();
    }

    public void RollNewWeather(int idx)
    {
        todayWeatherIdx = idx;
    }

    #endregion

    /* ======================================================================= */
    #region ▶ 자원 관련
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
    #region ▶ 크리처 관련
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

        if (currentDay >= nextInfectionUpgradeDay)
        {
            var cfg = Resources.Load<GameConfig>("GameConfig");
            infectionStage = Mathf.Min(infectionStage + 1, cfg.maxInfectionStage);
            nextInfectionUpgradeDay += cfg.infectionStagePeriodDays;
        }
    }

    #endregion

    /* ======================================================================= */
    #region ▶ 성과 집계
    /* ======================================================================= */

    public void ApplyDayResults(int earnCredit, int earnPopularity, int deltaReputation, int earnedScore)
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

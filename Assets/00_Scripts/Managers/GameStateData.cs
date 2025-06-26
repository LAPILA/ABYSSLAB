using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[DefaultExecutionOrder(-90)]
public class GameStateData : SerializedMonoBehaviour
{
    public static GameStateData I { get; private set; }

    /* ─────────────────────────────
     * Progress / Phase
     * ───────────────────────────── */
    [TitleGroup("진행"), ReadOnly, ShowInInspector] public int currentDay = 1;
    [ReadOnly, ShowInInspector] public GamePhase currentPhase = GamePhase.Title;
    [ReadOnly, ShowInInspector] public int todayWeatherIdx;
    [ReadOnly, ShowInInspector, LabelText("오늘 날씨 데이터")]
    public DeepSeaWeatherData TodayWeatherData
    {
        get
        {
            var cfg = GameManager.Instance?.CFG;
            return (cfg != null && todayWeatherIdx >= 0 && todayWeatherIdx < cfg.weatherTable.Count)
                 ? cfg.weatherTable[todayWeatherIdx] : null;
        }
    }

    /* ─────────────────────────────
     * Resources
     * ───────────────────────────── */
    [TitleGroup("자원"), ReadOnly, ShowInInspector] public int Credit { get; private set; }
    [TitleGroup("자원"), ReadOnly, ShowInInspector] public int Reputation { get; private set; }
    public event Action<int> OnCreditChanged, OnReputationChanged;

    /* ─────────────────────────────
     * Creatures
     * ───────────────────────────── */
    [TitleGroup("크리처"), ListDrawerSettings(ShowFoldout = true)]
    [ShowInInspector] public readonly List<string> ownedCreatures = new();
    [ListDrawerSettings(ShowFoldout = true)]
    [ShowInInspector] public readonly List<string> exhibitedCreatures = new();
    [ListDrawerSettings(ShowFoldout = true)]
    [ShowInInspector] public readonly List<string> managementCreatures = new();

    /* ─────────────────────────────
     * Score & Rank
     * ───────────────────────────── */
    [TitleGroup("성과"), ReadOnly, ShowInInspector] public int todayResearchPoint { get; private set; }
    [ReadOnly, ShowInInspector] public int kpiScore { get; private set; }

    public event Action<int> OnKpiChanged;
    public event Action<string> OnRankChanged;

    [ReadOnly, ShowInInspector, LabelText("현재 랭크")]
    public string CurrentRank
    {
        get
        {
            var cfg = GameManager.Instance?.CFG;
            if (cfg == null) return "F";
            int[] c = cfg.KPIScore;
            return kpiScore >= c[^1] ? "A" :
                   kpiScore >= c[^2] ? "B" :
                   kpiScore >= c[^3] ? "C" :
                   kpiScore >= c[^4] ? "D" : "F";
        }
    }

    // Life-cycle
    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this; DontDestroyOnLoad(gameObject);
        ResetGameState();
    }

    // Global Reset
    public void ResetGameState()
    {
        var cfg = GameManager.Instance?.CFG;
        if (cfg == null) { Debug.LogError("[GS] CFG is null"); return; }

        currentDay = 1;
        currentPhase = GamePhase.Title;
        todayWeatherIdx = 0;

        Credit = cfg.initialCredit;
        Reputation = cfg.initialReputation;
        todayResearchPoint = 0;
        kpiScore = 0;

        ownedCreatures.Clear();
        exhibitedCreatures.Clear();
        managementCreatures.Clear();

        Debug.Log("[GS] ResetGameState");
        NotifyRankIfChanged();
    }

    // Daily Reset
    public void ResetDaily()
    {
        todayResearchPoint = 0;
        exhibitedCreatures.Clear();
        managementCreatures.Clear();
        Debug.Log("[GS] Daily Reset");
    }

    // Phase Helpers
    public void StartNewDay()
    {
        currentDay++;
        currentPhase = GamePhase.Day;
        ResetDaily();
        Debug.Log($"[GS] Start New Day – Day: {currentDay}");
    }

    public void StartNight()
    {
        currentPhase = GamePhase.Night;
        Debug.Log($"[GS] Start New Day – Night: {currentDay}");
    }

    public void SetWeather(int idx)
    {
        todayWeatherIdx = idx;
        Debug.Log($"[GS] todayWeatherIdx = {idx}");
    }
    // Resource
    public void AddCredit(int delta)
    {
        Credit += delta;
        OnCreditChanged?.Invoke(Credit);
        Debug.Log($"[GS] Credit += {delta} ▶ {Credit}");
    }

    public void AddReputation(int delta)
    {
        Reputation += delta;
        OnReputationChanged?.Invoke(Reputation);
        Debug.Log($"[GS] Reputation += {delta} ▶ {Reputation}");
    }

    // Score
    public void AddResearchPoint(int delta)
    {
        todayResearchPoint += delta;
        Debug.Log($"[GS] Research += {delta} ▶ {todayResearchPoint}");
    }

    public void AddKPIScore(int delta)
    {
        var prevRank = CurrentRank;
        kpiScore += delta;
        OnKpiChanged?.Invoke(kpiScore);
        Debug.Log($"[GS] KPI += {delta} ▶ {kpiScore}");
        NotifyRankIfChanged(prevRank);
    }

    private void NotifyRankIfChanged(string prevRank = null)
    {
        prevRank ??= CurrentRank;
        var newRank = CurrentRank;
        if (prevRank != newRank)
        {
            OnRankChanged?.Invoke(newRank);
            Debug.Log($"[GS] Rank Update  {prevRank} ➜ {newRank}");
        }
    }

    // Creature
    public void AddOwnedCreature(string id)
    {
        if (ownedCreatures.Contains(id)) return;
        ownedCreatures.Add(id);
        Debug.Log($"[GS] Add Owned Creature – {id}");
    }
    public void RemoveOwnedCreature(string id)
    {
        if (ownedCreatures.Remove(id))
            Debug.Log($"[GS] Remove Creature – {id}");
    }
    public void AddExhibitedCreature(string id)
    {
        if (exhibitedCreatures.Contains(id)) return;
        exhibitedCreatures.Add(id);
        Debug.Log($"[GS] Exhibited Creature – {id}");
    }

    //Odin Debug Buttons
#if UNITY_EDITOR
    [Button("더미 Creature 추가")] private void _dbgAddCreature() => AddOwnedCreature($"MON_{UnityEngine.Random.Range(1, 99):D2}");
    [Button("Credit +100")] private void _dbgCredit() => AddCredit(100);
    [Button("Reputation +10")] private void _dbgRep() => AddReputation(10);
    [Button("Research +50")] private void _dbgRes() => AddResearchPoint(50);
#endif
}

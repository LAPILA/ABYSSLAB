//==============================================================================
// GameStateData
// - 런타임 전체 게임 상태 관리 (싱글톤)
// - 자원(Credit, Reputation), 몬스터, 연구점수, KPI 점수, 페이즈
//==============================================================================

using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-90)]
public class GameStateData : SerializedMonoBehaviour
{
    public static GameStateData I { get; private set; }

    // ===== 1. 게임 진행/페이즈 =====
    [TitleGroup("진행")]
    [PropertyOrder(0), ReadOnly, ShowInInspector] public int currentDay = 1;
    [PropertyOrder(1), ReadOnly, ShowInInspector] public GamePhase currentPhase = GamePhase.Title;
    [PropertyOrder(2), ReadOnly, ShowInInspector] public int todayWeatherIdx = 0;

    // ===== 2. 자원(Credit, Reputation) =====
    [TitleGroup("자원")]
    [ReadOnly, ShowInInspector] public int Credit { get; private set; }
    [ReadOnly, ShowInInspector] public int Reputation { get; private set; }

    public event Action<int> OnCreditChanged;
    public event Action<int> OnReputationChanged;

    // ===== 3. 크리처 현황 =====
    [TitleGroup("크리처")]
    [ShowInInspector] public readonly List<string> ownedCreatures = new();   // 보유중
    [ShowInInspector] public readonly List<string> exhibitedToday = new();   // 오늘 전시/판매

    // ===== 4. 연구/성과 관련 =====
    [TitleGroup("성과")]
    [ReadOnly, ShowInInspector] public int todayResearchPoint { get; private set; }
    [ReadOnly, ShowInInspector] public int kpiScore { get; private set; } // 정부 심사 등급용 (누적 또는 이번주)

    //등급 문자로 바로 접근
    public string CurrentRank
    {
        get
        {
            var cfg = GameManager.Instance?.CFG;
            if (cfg == null) return "F";
            int[] cuts = cfg.rankCutScores;
            if (kpiScore >= cuts[^1]) return "A";
            if (kpiScore >= cuts[^2]) return "B";
            if (kpiScore >= cuts[^3]) return "C";
            if (kpiScore >= cuts[^4]) return "D";
            return "F";
        }
    }

    /* ====== 초기화 & 리셋 ====== */
    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
        ResetGameState();
    }

    private void ResetGameState()
    {
        var cfg = GameManager.Instance?.CFG;
        if (cfg == null)
        {
            Debug.LogError("[GameStateData] GameManager.Instance.CFG is null!");
            return;
        }

        currentDay = 1;
        currentPhase = GamePhase.Title;
        Credit = cfg.initialCredit;
        Reputation = cfg.initialReputation;
        todayResearchPoint = 0;
        kpiScore = 0;
        ownedCreatures.Clear();
        exhibitedToday.Clear();
    }

    // ====== 자원 관련 ======
    public void AddCredit(int delta)
    {
        Credit += delta;
        OnCreditChanged?.Invoke(Credit);
    }
    public void AddReputation(int delta)
    {
        Reputation += delta;
        OnReputationChanged?.Invoke(Reputation);
    }

    // ====== 연구 포인트/성과 ======
    public void AddResearchPoint(int delta)
    {
        todayResearchPoint += delta;
        // HUD, 로그, 보상 트리거 등 필요시 추가
    }
    private void ResetTodayResearchPoint()
    {
        todayResearchPoint = 0;
    }
    public void AddKPIScore(int delta)
    {
        kpiScore += delta;
        // HUD/등급 UI 갱신 등 필요시 추가
    }

    // ====== 크리처 관련 ======
    public void AddOwnedCreature(string id)
    {
        if (!ownedCreatures.Contains(id))
            ownedCreatures.Add(id);
    }
    public void RemoveOwnedCreature(string id)
    {
        ownedCreatures.Remove(id);
    }
    public void AddExhibitedCreature(string id)
    {
        if (!exhibitedToday.Contains(id))
            exhibitedToday.Add(id);
    }
    public void ResetExhibitedToday()
    {
        exhibitedToday.Clear();
    }

    // ====== 일자/페이즈 전환 ======
    // ====== 매일 리셋 공통 ======
    public void ResetDayNightCommon()
    {
        //리셋해야 할 데이터
        ResetTodayResearchPoint();
        ResetExhibitedToday();
    }

    // ====== 낮 시작 ======
    public void StartNewDay()
    {
        currentDay++;
        currentPhase = GamePhase.Day;
        ResetDayNightCommon();
        // 낮 특화 초기화있으면 추가
    }

    // ====== 밤 시작 ======
    public void StartNight()
    {
        currentPhase = GamePhase.Night;
        ResetDayNightCommon();
    }


    // ====== 날씨 변경 ======
    public void SetWeather(int idx)
    {
        todayWeatherIdx = idx;
    }
}

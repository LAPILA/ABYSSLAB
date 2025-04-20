//==============================================================================
// 게임의 현재 상태 데이터를 저장 및 관리하는 클래스 (런타임 상태 추적)
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateData : MonoBehaviour
{
    public static GameStateData Instance { get; private set; }

    [Header("■ 진행 정보")]
    public int currentDay = 1;
    public GamePhase currentPhase = GamePhase.None;

    [Header("■ 플레이어 자원")]
    public float credit;
    public float popularity;
    public float reputation;

    [Header("■ 크리처 관리")]
    public List<string> ownedCreatureIDs = new List<string>();
    public List<string> exhibitedCreatureIDs = new List<string>();

    [Header("■ 성과 및 기록")]
    public int successCount;
    public int failureCount;
    public int totalScore;

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // GameConfig로부터 초기 자원 로드
        var cfg = Resources.Load<GameConfig>("GameConfig");
        if (cfg != null)
        {
            credit = cfg.initialCredit;
            popularity = cfg.initialPopularity;
            reputation = cfg.initialReputation;
        }
    }

    //--- 게임 시작 혹은 새 게임 시 호출 ---
    public void ResetGameState()
    {
        currentDay = 1;
        currentPhase = GamePhase.None;

        var cfg = Resources.Load<GameConfig>("GameConfig");
        credit = cfg != null ? cfg.initialCredit : 0f;
        popularity = cfg != null ? cfg.initialPopularity : 0f;
        reputation = cfg != null ? cfg.initialReputation : 0f;

        ownedCreatureIDs.Clear();
        exhibitedCreatureIDs.Clear();

        successCount = 0;
        failureCount = 0;
        totalScore = 0;
    }

    //--- 낮이 시작될 때 호출 ---
    public void StartNewDay()
    {
        currentDay++;
        exhibitedCreatureIDs.Clear();
        currentPhase = GamePhase.Day;
    }

    //--- 크리처 보유 처리 ---
    public void AddOwnedCreature(string id)
    {
        if (!ownedCreatureIDs.Contains(id))
            ownedCreatureIDs.Add(id);
    }

    //--- 당일 전시 등록 처리 ---
    public void AddExhibitedCreature(string id)
    {
        if (!exhibitedCreatureIDs.Contains(id))
            exhibitedCreatureIDs.Add(id);
    }

    //--- 낮 결과 반영 (수익·점수 등) ---
    public void ApplyDayResults(float earnCredit, float earnPopularity, float deltaReputation, int earnedScore)
    {
        credit += earnCredit;
        popularity += earnPopularity;
        reputation += deltaReputation;
        totalScore += earnedScore;
        successCount++;
    }

    //--- 실패 처리 (당일 사망 시) ---
    public void RegisterFailure(int penaltyScore = 0, float penaltyReputation = 5f)
    {
        failureCount++;
        totalScore -= penaltyScore;
        reputation -= penaltyReputation;
    }
}

//==============================================================================
// ������ ���� ���� �����͸� ���� �� �����ϴ� Ŭ���� (��Ÿ�� ���� ����)
//==============================================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateData : MonoBehaviour
{
    public static GameStateData Instance { get; private set; }

    [Header("�� ���� ����")]
    public int currentDay = 1;
    public GamePhase currentPhase = GamePhase.None;

    [Header("�� �÷��̾� �ڿ�")]
    public float credit;
    public float popularity;
    public float reputation;

    [Header("�� ũ��ó ����")]
    public List<string> ownedCreatureIDs = new List<string>();
    public List<string> exhibitedCreatureIDs = new List<string>();

    [Header("�� ���� �� ���")]
    public int successCount;
    public int failureCount;
    public int totalScore;

    private void Awake()
    {
        // �̱��� ����
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // GameConfig�κ��� �ʱ� �ڿ� �ε�
        var cfg = Resources.Load<GameConfig>("GameConfig");
        if (cfg != null)
        {
            credit = cfg.initialCredit;
            popularity = cfg.initialPopularity;
            reputation = cfg.initialReputation;
        }
    }

    //--- ���� ���� Ȥ�� �� ���� �� ȣ�� ---
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

    //--- ���� ���۵� �� ȣ�� ---
    public void StartNewDay()
    {
        currentDay++;
        exhibitedCreatureIDs.Clear();
        currentPhase = GamePhase.Day;
    }

    //--- ũ��ó ���� ó�� ---
    public void AddOwnedCreature(string id)
    {
        if (!ownedCreatureIDs.Contains(id))
            ownedCreatureIDs.Add(id);
    }

    //--- ���� ���� ��� ó�� ---
    public void AddExhibitedCreature(string id)
    {
        if (!exhibitedCreatureIDs.Contains(id))
            exhibitedCreatureIDs.Add(id);
    }

    //--- �� ��� �ݿ� (���͡����� ��) ---
    public void ApplyDayResults(float earnCredit, float earnPopularity, float deltaReputation, int earnedScore)
    {
        credit += earnCredit;
        popularity += earnPopularity;
        reputation += deltaReputation;
        totalScore += earnedScore;
        successCount++;
    }

    //--- ���� ó�� (���� ��� ��) ---
    public void RegisterFailure(int penaltyScore = 0, float penaltyReputation = 5f)
    {
        failureCount++;
        totalScore -= penaltyScore;
        reputation -= penaltyReputation;
    }
}

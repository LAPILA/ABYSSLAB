using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "ABYSS_LAB/GameConfig", fileName = "GameConfig")]
public class GameConfig : ScriptableObject
{
    [Title("�� �̸� ����")]
    [BoxGroup("Level Scenes")] public string titleSceneName;
    [BoxGroup("Level Scenes")] public string tutorialSceneName;
    [BoxGroup("Level Scenes")] public string daySceneName;
    [BoxGroup("Level Scenes")] public string nightSceneName;

    [Title("�ʱ� �ڿ� ����")]
    [BoxGroup("Initial Resources"), Tooltip("���� ũ����")]
    public int initialCredit = 0;
    [BoxGroup("Initial Resources"), Tooltip("���� ���� (Reputation)")]
    public int initialReputation = 0;

    [Title("����/ȯ��")]
    [BoxGroup("����")]
    public List<DeepSeaWeatherData> weatherTable = new List<DeepSeaWeatherData>();
    
    // F D C B A
    [Title("��� ��ũ")]
    [BoxGroup("Rank"), Tooltip("KPI �����ũ �� ���� (F~A)")]
    public int[] KPIScore = new int[] { 0, 1000, 3000, 6000, 10000 };

    [Title("�ŷ�")]
    [BoxGroup("Economy"), Tooltip("�⺻ ���� �Ǹ� ����(�ּ�)")]
    public int defaultMonsterSellMin = 100;
    [BoxGroup("Economy"), Tooltip("�⺻ ���� �Ǹ� ����(�ִ�)")]
    public int defaultMonsterSellMax = 10000;

    // ���� �� �÷��̾� �̸� or ������ 
    [Title("������/�÷��̾�")]
    [BoxGroup("Lab & Player"), Tooltip("�⺻ ������ ��Ī")]
    public string defaultLabName = "AbyssLab";
    [BoxGroup("Lab & Player"), Tooltip("�÷��̾� ����")]
    public string defaultPlayerName = "Operator";

    [Title("����/��Ÿ")]
    [BoxGroup("FX & Misc"), Tooltip("�� ��ȯ ���̵� �ð�(��)")]
    public float fadeDuration = 1.5f;
    [BoxGroup("FX & Misc"), Tooltip("���� ���� ������(��)"), Range(0f, 2f)]
    public float startDelay = 1.0f;
}

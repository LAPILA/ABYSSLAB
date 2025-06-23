using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "ABYSS_LAB/GameConfig", fileName = "GameConfig")]
public class GameConfig : ScriptableObject
{
    [Title("�� �̸� ����")]
    [BoxGroup("Level Scenes")] public string titleSceneName;
    [BoxGroup("Level Scenes")] public string daySceneName;
    [BoxGroup("Level Scenes")] public string nightSceneName;

    [Title("�ʱ� �ڿ� ����")]
    [BoxGroup("Initial Resources"), Tooltip("���� ũ����"), MinValue(0), MaxValue(9999)]
    public int initialCredit = 50;
    [BoxGroup("Initial Resources"), Tooltip("���� ���� (Reputation)"), MinValue(-100), MaxValue(100)]
    public int initialReputation = 0;

    [Title("�ѵ�/�뷱��")]
    [BoxGroup("Limits"), Tooltip("�ִ� ���� ũ��ó ��"), MinValue(0), MaxValue(100)]
    public int maxCreatures = 12;
    [BoxGroup("Limits"), Tooltip("�� �ִ� ����"), MinValue(1), MaxValue(32)]
    public int maxRooms = 6;

    [Title("����/ȯ��")]
    [BoxGroup("����")]
    public List<DeepSeaWeatherData> weatherTable = new List<DeepSeaWeatherData>();

    [Title("��ũ/�ɻ�")]
    [BoxGroup("Rank & Audit"), Tooltip("��ũ �� ���� (F~A)")]
    public int[] rankCutScores = new int[] { 0, 1000, 3000, 6000, 10000 };

    [Title("������/�÷��̾�")]
    [BoxGroup("Lab & Player"), Tooltip("�⺻ ������ ��Ī")]
    public string defaultLabName = "AbyssLab";
    [BoxGroup("Lab & Player"), Tooltip("�÷��̾� ����")]
    public string defaultPlayerName = "Operator";

    [Title("����/��Ÿ")]
    [BoxGroup("FX & Misc"), Tooltip("�� ��ȯ ���̵� �ð�(��)"), Range(0f, 3f)]
    public float fadeDuration = 1.5f;
    [BoxGroup("FX & Misc"), Tooltip("���� ���� ������(��)"), Range(0f, 2f)]
    public float startDelay = 1.0f;
}

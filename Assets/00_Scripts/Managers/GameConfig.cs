//==============================================================================
// ������ ���� �������� �����ϴ� ScriptableObject
// ��/�� �ð�, �� �̸�, �ʱ� �ڿ����α⵵�����ǡ��ִ� ũ��ó �� �� ����
//==============================================================================

using System;
using UnityEngine;

[CreateAssetMenu(menuName = "ABYSS_LAB/GameConfig", fileName = "GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("�� �ð� ���� ����")]
    public float dayDuration = 180f;
    public float nightDuration = 300f;
    public float fadeDuration = 1.5f;
    public float startDelay = 1.0f;

    [Header("�� �� �̸�")]
    public string daySceneName = "DayScene";
    public string nightSceneName = "NightScene";

    [Header("�� �ʱ� ���°�")]
    public float initialCredit = 1000f;
    public float initialPopularity = 0f;
    public float initialReputation = 0f;
    public int maxCreatures = 5;
}

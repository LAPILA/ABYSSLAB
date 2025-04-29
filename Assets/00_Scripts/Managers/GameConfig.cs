//==============================================================================
// ������ ���� �������� �����ϴ� ScriptableObject
// ��/�� �ð�, �� �̸�, �ʱ� �ڿ����α⵵�����ǡ��ִ� ũ��ó �� �� ����
//==============================================================================

using System;
using UnityEngine;

[CreateAssetMenu(menuName = "ABYSS_LAB/GameConfig", fileName = "GameConfig")]
public class GameConfig : ScriptableObject
{
    /*-------- Time --------*/
    [Header("�� �ð�")]
    [Tooltip("1�ʿ� ���� �� �� ���� �帣�°�?")]
    public int minutesPerRealSecond = 10; //���� 1�� = minutesPerRealSecond�� : �� ���� ���� Ÿ�̸�
    public int dayStartMinutes = 540;   // ���� 9�� (9*60=540)
    public int dayEndMinutes = 1080;    // ���� 6�� (18*60=1080)
    public int nightStartMinutes = 1300;
    public int nightEndMinutes = 360;    // ���� 1�� (1*60=60)

    public float fadeDuration = 1.5f;
    public float startDelay = 1.0f;
    public int weekendInterval = 7; //7�ϸ��� �� ��
    public int storyIntervalDays = 3; //���丮 �ƾ�/�̺�Ʈ �ֱ�

    /*-------- Scene --------*/
    [Header("�� �� �̸�")]
    public string titleSceneName = "";
    public string daySceneName = "";
    public string nightSceneName = "";

    /*-------- Economy / Score --------*/
    [Header("�� �ʱ� �ڿ�")]
    public int initialCredit = 1000;
    public int initialPopularity = 0;
    public int initialReputation = 0;

    /*-------- Creature --------*/
    [Header("�� ũ��ó �ѵ�")]
    public int demoMaxCreatures = 10;
    public int fullMaxCreatures = 40;

    /*-------- Infection --------*/
    [Header("�� ����")]
    public int startInfectionStage = 0;
    public int maxInfectionStage = 5;
    public int infectionStagePeriodDays = 14; // 2�ָ��� +1�ܰ�

    /*-------- Weather --------*/
    [Header("�� ���� ������ & ���̺�")]
    public Sprite[] weatherIcons;
}

using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "ABYSS_LAB/GameConfig", fileName = "GameConfig")]
public class GameConfig : ScriptableObject
{
    [Title("�ð� ����")]
    [BoxGroup("�ð�")]
    [Tooltip("1�ʿ� ���� �� �� ���� �帣�°�? (�� ���� ���� �ð�)")]
    [Range(1, 60)]
    public int minutesPerRealSecond = 10;

    [BoxGroup("�ð�")]
    [Tooltip("�� ���� �ð� (�� ����, 9*60=540)")]
    [Range(0, 1439)]
    public int dayStartMinutes = 540;

    [BoxGroup("�ð�")]
    [Tooltip("�� ���� �ð� (18*60=1080)")]
    [Range(0, 1439)]
    public int dayEndMinutes = 1080;

    [BoxGroup("�ð�")]
    [Tooltip("�� ���� �ð� (��: 21:00=1260)")]
    [Range(0, 1439)]
    public int nightStartMinutes = 1260;

    [BoxGroup("�ð�")]
    [Tooltip("�� ���� �ð� (��: 6:00=360)")]
    [Range(0, 1439)]
    public int nightEndMinutes = 360;

    [BoxGroup("�ð�")]
    [Tooltip("�� ��ȯ ���̵� �ð� (��)")]
    [Range(0f, 5f)]
    public float fadeDuration = 1.5f;

    [BoxGroup("�ð�")]
    [Tooltip("���� ���� ������ (��)")]
    [Range(0f, 5f)]
    public float startDelay = 1.0f;

    [BoxGroup("�ð�")]
    [Tooltip("������ �ֱ� (7=7�ϸ��� �� ��)")]
    [Range(1, 30)]
    public int weekendInterval = 7;

    [BoxGroup("�ð�")]
    [Tooltip("���丮 �̺�Ʈ ���� (�� ����)")]
    [Range(1, 30)]
    public int storyIntervalDays = 3;

    [Title("�� �̸� ����")]
    [BoxGroup("�� �̸�")]
    public string titleSceneName;

    [BoxGroup("�� �̸�")]
    public string daySceneName;

    [BoxGroup("�� �̸�")]
    public string nightSceneName;

    [Title("�ʱ� �ڿ� ����")]
    [BoxGroup("�ʱ� �ڿ�")]
    [Tooltip("���� ũ����")]
    [MinValue(0)]
    public int initialCredit = 1000;

    [BoxGroup("�ʱ� �ڿ�")]
    [Tooltip("���� �α⵵")]
    [MinValue(0)]
    public int initialPopularity = 0;

    [BoxGroup("�ʱ� �ڿ�")]
    [Tooltip("���� ����")]
    [MinValue(0)]
    public int initialReputation = 0;

    [Title("ũ��ó �ѵ� ����")]
    [BoxGroup("ũ��ó")]
    [Tooltip("���� ���� �ִ� ũ��ó ��")]
    [MinValue(0)]
    public int demoMaxCreatures = 10;

    [BoxGroup("ũ��ó")]
    [Tooltip("Ǯ ���� �ִ� ũ��ó ��")]
    [MinValue(0)]
    public int fullMaxCreatures = 40;

    [Title("���� �ܰ� ����")]
    [BoxGroup("����")]
    [Tooltip("�ʱ� ���� �ܰ�")]
    [Range(0, 5)]
    public int startInfectionStage = 0;

    [BoxGroup("����")]
    [Tooltip("�ִ� ���� �ܰ�")]
    [Range(1, 10)]
    public int maxInfectionStage = 5;

    [BoxGroup("����")]
    [Tooltip("�� �ϸ��� ���� �ܰ� ���? (����: ��)")]
    [Range(1, 30)]
    public int infectionStagePeriodDays = 14;

    [Title("���� ����")]
    [BoxGroup("����")]
    [Tooltip("���� ������ �迭")]
    public Sprite[] weatherIcons;
}

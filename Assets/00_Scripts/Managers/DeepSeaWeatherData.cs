using UnityEngine;
using Sirenix.OdinInspector;
using System;

public enum WeatherEffectType { None, StrongCurrent, MicrobeFlux, SeabedQuake, ColdCurrent, Thunderstorm }

[CreateAssetMenu(menuName = "ABYSS_LAB/DeepSeaWeatherData", fileName = "DeepSeaWeatherData")]
public class DeepSeaWeatherData : ScriptableObject
{
    /* ���������������������������� ��Ÿ ���������������������������� */
    [Title("�⺻ ����"), LabelWidth(90)]
    public string weatherName;
    [TextArea] public string description;
    public Sprite icon;

    /* ���������������������������� �ý��� ���������������������������� */
    [Title("�ý���")]
    [EnumToggleButtons] public WeatherEffectType effectType = WeatherEffectType.None;
    [Tooltip("���� ���൵(���丮 é��) �� unlockStage ���� ����")]
    public int unlockStage = 0;
    [Tooltip("���� Ŭ���� ���� Ȯ�� ������")]
    [MinValue(1)]
    public int weight = 1;

    /* ���������������������������� ���� ���������������������������� */
    [Title("���� / ����Ʈ Tag")]
    [TableList] public string[] extraEffects;
}

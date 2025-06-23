using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public enum WeatherEffectType
{
    None,
    StrongCurrent,   // ���� �ݷ�
    MicrobeFlux,     // �̻��� ����
    SeabedQuake,     // ���� ����
    ColdCurrent,     // ������ ����
    Thunderstorm,    // ����
}

[CreateAssetMenu(menuName = "ABYSS_LAB/DeepSeaWeatherData", fileName = "DeepSeaWeatherData")]
public class DeepSeaWeatherData : ScriptableObject
{
    [Title("�⺻ ����")]
    [BoxGroup("�⺻")]
    public string weatherName;

    [BoxGroup("�⺻")]
    [TextArea]
    public string description;

    [BoxGroup("�⺻")]
    public Sprite icon;

    [BoxGroup("Ÿ��")]
    public WeatherEffectType effectType;

    [BoxGroup("����")]
    [Tooltip("���� ���� �� ���� ȿ��")]
    public AudioClip sfx;
}

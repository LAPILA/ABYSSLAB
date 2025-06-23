using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public enum WeatherEffectType
{
    None,
    StrongCurrent,   // 심해 격류
    MicrobeFlux,     // 미생물 증가
    SeabedQuake,     // 해저 지진
    ColdCurrent,     // 차가운 조류
    Thunderstorm,    // 낙뢰
}

[CreateAssetMenu(menuName = "ABYSS_LAB/DeepSeaWeatherData", fileName = "DeepSeaWeatherData")]
public class DeepSeaWeatherData : ScriptableObject
{
    [Title("기본 정보")]
    [BoxGroup("기본")]
    public string weatherName;

    [BoxGroup("기본")]
    [TextArea]
    public string description;

    [BoxGroup("기본")]
    public Sprite icon;

    [BoxGroup("타입")]
    public WeatherEffectType effectType;

    [BoxGroup("연출")]
    [Tooltip("게임 시작 시 사운드 효과")]
    public AudioClip sfx;
}

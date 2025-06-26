using UnityEngine;
using Sirenix.OdinInspector;
using System;

public enum WeatherEffectType { None, StrongCurrent, MicrobeFlux, SeabedQuake, ColdCurrent, Thunderstorm }

[CreateAssetMenu(menuName = "ABYSS_LAB/DeepSeaWeatherData", fileName = "DeepSeaWeatherData")]
public class DeepSeaWeatherData : ScriptableObject
{
    /* ────────────── 메타 ────────────── */
    [Title("기본 정보"), LabelWidth(90)]
    public string weatherName;
    [TextArea] public string description;
    public Sprite icon;

    /* ────────────── 시스템 ────────────── */
    [Title("시스템")]
    [EnumToggleButtons] public WeatherEffectType effectType = WeatherEffectType.None;
    [Tooltip("게임 진행도(스토리 챕터) ≥ unlockStage 이후 등장")]
    public int unlockStage = 0;
    [Tooltip("값이 클수록 뽑힐 확률 높아짐")]
    [MinValue(1)]
    public int weight = 1;

    /* ────────────── 연출 ────────────── */
    [Title("연출 / 이펙트 Tag")]
    [TableList] public string[] extraEffects;
}

using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "ABYSS_LAB/GameConfig", fileName = "GameConfig")]
public class GameConfig : ScriptableObject
{
    [Title("씬 이름 설정")]
    [BoxGroup("Level Scenes")] public string titleSceneName;
    [BoxGroup("Level Scenes")] public string daySceneName;
    [BoxGroup("Level Scenes")] public string nightSceneName;

    [Title("초기 자원 설정")]
    [BoxGroup("Initial Resources"), Tooltip("시작 크레딧"), MinValue(0), MaxValue(9999)]
    public int initialCredit = 50;
    [BoxGroup("Initial Resources"), Tooltip("시작 평판 (Reputation)"), MinValue(-100), MaxValue(100)]
    public int initialReputation = 0;

    [Title("한도/밸런스")]
    [BoxGroup("Limits"), Tooltip("최대 보유 크리처 수"), MinValue(0), MaxValue(100)]
    public int maxCreatures = 12;
    [BoxGroup("Limits"), Tooltip("방 최대 개수"), MinValue(1), MaxValue(32)]
    public int maxRooms = 6;

    [Title("날씨/환경")]
    [BoxGroup("날씨")]
    public List<DeepSeaWeatherData> weatherTable = new List<DeepSeaWeatherData>();

    [Title("랭크/심사")]
    [BoxGroup("Rank & Audit"), Tooltip("랭크 컷 점수 (F~A)")]
    public int[] rankCutScores = new int[] { 0, 1000, 3000, 6000, 10000 };

    [Title("연구소/플레이어")]
    [BoxGroup("Lab & Player"), Tooltip("기본 연구소 명칭")]
    public string defaultLabName = "AbyssLab";
    [BoxGroup("Lab & Player"), Tooltip("플레이어 직함")]
    public string defaultPlayerName = "Operator";

    [Title("연출/기타")]
    [BoxGroup("FX & Misc"), Tooltip("씬 전환 페이드 시간(초)"), Range(0f, 3f)]
    public float fadeDuration = 1.5f;
    [BoxGroup("FX & Misc"), Tooltip("게임 시작 딜레이(초)"), Range(0f, 2f)]
    public float startDelay = 1.0f;
}

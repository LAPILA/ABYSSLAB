using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "ABYSS_LAB/GameConfig", fileName = "GameConfig")]
public class GameConfig : ScriptableObject
{
    [Title("씬 이름 설정")]
    [BoxGroup("Level Scenes")] public string titleSceneName;
    [BoxGroup("Level Scenes")] public string tutorialSceneName;
    [BoxGroup("Level Scenes")] public string daySceneName;
    [BoxGroup("Level Scenes")] public string nightSceneName;

    [Title("초기 자원 설정")]
    [BoxGroup("Initial Resources"), Tooltip("시작 크레딧")]
    public int initialCredit = 0;
    [BoxGroup("Initial Resources"), Tooltip("시작 평판 (Reputation)")]
    public int initialReputation = 0;

    [Title("날씨/환경")]
    [BoxGroup("날씨")]
    public List<DeepSeaWeatherData> weatherTable = new List<DeepSeaWeatherData>();
    
    // F D C B A
    [Title("기업 랭크")]
    [BoxGroup("Rank"), Tooltip("KPI 기업랭크 컷 점수 (F~A)")]
    public int[] KPIScore = new int[] { 0, 1000, 3000, 6000, 10000 };

    [Title("거래")]
    [BoxGroup("Economy"), Tooltip("기본 몬스터 판매 가격(최소)")]
    public int defaultMonsterSellMin = 100;
    [BoxGroup("Economy"), Tooltip("기본 몬스터 판매 가격(최대)")]
    public int defaultMonsterSellMax = 10000;

    // 게임 속 플레이어 이름 or 연구소 
    [Title("연구소/플레이어")]
    [BoxGroup("Lab & Player"), Tooltip("기본 연구소 명칭")]
    public string defaultLabName = "AbyssLab";
    [BoxGroup("Lab & Player"), Tooltip("플레이어 직함")]
    public string defaultPlayerName = "Operator";

    [Title("연출/기타")]
    [BoxGroup("FX & Misc"), Tooltip("씬 전환 페이드 시간(초)")]
    public float fadeDuration = 1.5f;
    [BoxGroup("FX & Misc"), Tooltip("게임 시작 딜레이(초)"), Range(0f, 2f)]
    public float startDelay = 1.0f;
}

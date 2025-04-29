using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "ABYSS_LAB/GameConfig", fileName = "GameConfig")]
public class GameConfig : ScriptableObject
{
    [Title("시간 설정")]
    [BoxGroup("시간")]
    [Tooltip("1초에 게임 내 몇 분이 흐르는가? (낮 전용 가상 시간)")]
    [Range(1, 60)]
    public int minutesPerRealSecond = 10;

    [BoxGroup("시간")]
    [Tooltip("낮 시작 시각 (분 단위, 9*60=540)")]
    [Range(0, 1439)]
    public int dayStartMinutes = 540;

    [BoxGroup("시간")]
    [Tooltip("낮 종료 시각 (18*60=1080)")]
    [Range(0, 1439)]
    public int dayEndMinutes = 1080;

    [BoxGroup("시간")]
    [Tooltip("밤 시작 시각 (예: 21:00=1260)")]
    [Range(0, 1439)]
    public int nightStartMinutes = 1260;

    [BoxGroup("시간")]
    [Tooltip("밤 종료 시각 (예: 6:00=360)")]
    [Range(0, 1439)]
    public int nightEndMinutes = 360;

    [BoxGroup("시간")]
    [Tooltip("씬 전환 페이드 시간 (초)")]
    [Range(0f, 5f)]
    public float fadeDuration = 1.5f;

    [BoxGroup("시간")]
    [Tooltip("게임 시작 딜레이 (초)")]
    [Range(0f, 5f)]
    public float startDelay = 1.0f;

    [BoxGroup("시간")]
    [Tooltip("일주일 주기 (7=7일마다 한 주)")]
    [Range(1, 30)]
    public int weekendInterval = 7;

    [BoxGroup("시간")]
    [Tooltip("스토리 이벤트 간격 (일 단위)")]
    [Range(1, 30)]
    public int storyIntervalDays = 3;

    [Title("씬 이름 설정")]
    [BoxGroup("씬 이름")]
    public string titleSceneName;

    [BoxGroup("씬 이름")]
    public string daySceneName;

    [BoxGroup("씬 이름")]
    public string nightSceneName;

    [Title("초기 자원 설정")]
    [BoxGroup("초기 자원")]
    [Tooltip("시작 크레딧")]
    [MinValue(0)]
    public int initialCredit = 1000;

    [BoxGroup("초기 자원")]
    [Tooltip("시작 인기도")]
    [MinValue(0)]
    public int initialPopularity = 0;

    [BoxGroup("초기 자원")]
    [Tooltip("시작 평판")]
    [MinValue(0)]
    public int initialReputation = 0;

    [Title("크리처 한도 설정")]
    [BoxGroup("크리처")]
    [Tooltip("데모 버전 최대 크리처 수")]
    [MinValue(0)]
    public int demoMaxCreatures = 10;

    [BoxGroup("크리처")]
    [Tooltip("풀 버전 최대 크리처 수")]
    [MinValue(0)]
    public int fullMaxCreatures = 40;

    [Title("감염 단계 설정")]
    [BoxGroup("감염")]
    [Tooltip("초기 감염 단계")]
    [Range(0, 5)]
    public int startInfectionStage = 0;

    [BoxGroup("감염")]
    [Tooltip("최대 감염 단계")]
    [Range(1, 10)]
    public int maxInfectionStage = 5;

    [BoxGroup("감염")]
    [Tooltip("몇 일마다 감염 단계 상승? (단위: 일)")]
    [Range(1, 30)]
    public int infectionStagePeriodDays = 14;

    [Title("날씨 설정")]
    [BoxGroup("날씨")]
    [Tooltip("날씨 아이콘 배열")]
    public Sprite[] weatherIcons;
}

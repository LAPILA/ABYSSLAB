//==============================================================================
// 게임의 고정 설정값을 관리하는 ScriptableObject
// 낮/밤 시간, 씬 이름, 초기 자원·인기도·평판·최대 크리처 수 등 포함
//==============================================================================

using System;
using UnityEngine;

[CreateAssetMenu(menuName = "ABYSS_LAB/GameConfig", fileName = "GameConfig")]
public class GameConfig : ScriptableObject
{
    /*-------- Time --------*/
    [Header("■ 시간")]
    [Tooltip("1초에 게임 내 몇 분이 흐르는가?")]
    public int minutesPerRealSecond = 10; //현실 1초 = minutesPerRealSecond분 : 낮 전용 가상 타이머
    public int dayStartMinutes = 540;   // 오전 9시 (9*60=540)
    public int dayEndMinutes = 1080;    // 오후 6시 (18*60=1080)
    public int nightStartMinutes = 1300;
    public int nightEndMinutes = 360;    // 오전 1시 (1*60=60)

    public float fadeDuration = 1.5f;
    public float startDelay = 1.0f;
    public int weekendInterval = 7; //7일마다 한 주
    public int storyIntervalDays = 3; //스토리 컷씬/이벤트 주기

    /*-------- Scene --------*/
    [Header("■ 씬 이름")]
    public string titleSceneName = "";
    public string daySceneName = "";
    public string nightSceneName = "";

    /*-------- Economy / Score --------*/
    [Header("■ 초기 자원")]
    public int initialCredit = 1000;
    public int initialPopularity = 0;
    public int initialReputation = 0;

    /*-------- Creature --------*/
    [Header("■ 크리처 한도")]
    public int demoMaxCreatures = 10;
    public int fullMaxCreatures = 40;

    /*-------- Infection --------*/
    [Header("■ 감염")]
    public int startInfectionStage = 0;
    public int maxInfectionStage = 5;
    public int infectionStagePeriodDays = 14; // 2주마다 +1단계

    /*-------- Weather --------*/
    [Header("■ 기후 아이콘 & 테이블")]
    public Sprite[] weatherIcons;
}

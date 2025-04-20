//==============================================================================
// 게임의 고정 설정값을 관리하는 ScriptableObject
// 낮/밤 시간, 씬 이름, 초기 자원·인기도·평판·최대 크리처 수 등 포함
//==============================================================================

using System;
using UnityEngine;

[CreateAssetMenu(menuName = "ABYSS_LAB/GameConfig", fileName = "GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("■ 시간 관련 설정")]
    public float dayDuration = 180f;
    public float nightDuration = 300f;
    public float fadeDuration = 1.5f;
    public float startDelay = 1.0f;

    [Header("■ 씬 이름")]
    public string daySceneName = "DayScene";
    public string nightSceneName = "NightScene";

    [Header("■ 초기 상태값")]
    public float initialCredit = 1000f;
    public float initialPopularity = 0f;
    public float initialReputation = 0f;
    public int maxCreatures = 5;
}

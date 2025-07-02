using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class WeatherManager : SerializedMonoBehaviour
{
    public static WeatherManager I { get; private set; }

    /* ───────── 테이블 ───────── */
    [Title("Weather Table")]
    [TableList(IsReadOnly = false, AlwaysExpanded = true)]
    [AssetsOnly, InlineEditor]
    public List<DeepSeaWeatherData> masterTable = new();

    /* ───────── 런타임 ───────── */
    [ShowInInspector, ReadOnly, LabelText("오늘 날씨")] private DeepSeaWeatherData today;
    public event Action<DeepSeaWeatherData> OnWeatherChanged;

    private void Awake() => I = this;

    /* ───────── 공용 메서드 ───────── */
    [Button("Roll Today Weather", ButtonSizes.Gigantic)]
    [GUIColor(0.4f, 0.9f, 1f)]
    private void RollTodayWeather()
    {
        /* 안전 체크 */
        if (GameStateData.I == null)
        {
            Debug.LogError("[Weather] GameStateData 싱글톤이 없습니다! 씬에 프리팹이 있는지 확인.");
            return;
        }
        if (masterTable == null || masterTable.Count == 0)
        {
            Debug.LogError("[Weather] masterTable 이 비어있습니다. SO를 리스트에 등록하세요.");
            return;
        }

        /* 1) 잠금 해제 필터링 */
        var list = GetUnlockedWeathers(GameStateData.I.currentDay);
        if (list.Count == 0)
        {
            Debug.LogWarning("[Weather] 현재 진행도에서 등장 가능한 날씨가 없습니다.");
            return;
        }

        /* 2) 확률 가중 랜덤 */
        today = GetRandomWeighted(list);

        /* 3) GameStateData 에 기록 */
        GameStateData.I.SetWeather(masterTable.IndexOf(today));

        /* 4) 효과 적용 & 이벤트 */
        ApplyWeatherEffects(today);
        OnWeatherChanged?.Invoke(today);

        Debug.Log($"[Weather] ▶ {today.weatherName}");
    }

    /* ───────── 보조 함수 ───────── */
    private List<DeepSeaWeatherData> GetUnlockedWeathers(int stage)
    {
        var unlocked = new List<DeepSeaWeatherData>();
        foreach (var w in masterTable)
            if (stage >= w.unlockStage) unlocked.Add(w);
        return unlocked;
    }

    private DeepSeaWeatherData GetRandomWeighted(List<DeepSeaWeatherData> pool)
    {
        int total = 0;
        foreach (var w in pool) total += w.weight;

        int pick = UnityEngine.Random.Range(0, total);
        foreach (var w in pool)
        {
            if ((pick -= w.weight) < 0) return w;
        }
        return pool[0];
    }

    private void ApplyWeatherEffects(DeepSeaWeatherData data)
    {
        Debug.Log($"[Weather] Apply {data.weatherName}  (Type: {data.effectType})");
    }

    [Button("Reset Effects"), GUIColor(1f, 0.6f, 0.6f)]
    private void ResetWeatherEffects()
    {
        Debug.Log("[Weather] Reset (stub)");
    }
}

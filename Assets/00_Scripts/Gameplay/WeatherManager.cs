using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class WeatherManager : SerializedMonoBehaviour
{
    public static WeatherManager I { get; private set; }

    /* ������������������ ���̺� ������������������ */
    [Title("Weather Table")]
    [TableList(IsReadOnly = false, AlwaysExpanded = true)]
    [AssetsOnly, InlineEditor]
    public List<DeepSeaWeatherData> masterTable = new();

    /* ������������������ ��Ÿ�� ������������������ */
    [ShowInInspector, ReadOnly, LabelText("���� ����")] private DeepSeaWeatherData today;
    public event Action<DeepSeaWeatherData> OnWeatherChanged;

    private void Awake() => I = this;

    /* ������������������ ���� �޼��� ������������������ */
    [Button("Roll Today Weather", ButtonSizes.Gigantic)]
    [GUIColor(0.4f, 0.9f, 1f)]
    private void RollTodayWeather()
    {
        /* ���� üũ */
        if (GameStateData.I == null)
        {
            Debug.LogError("[Weather] GameStateData �̱����� �����ϴ�! ���� �������� �ִ��� Ȯ��.");
            return;
        }
        if (masterTable == null || masterTable.Count == 0)
        {
            Debug.LogError("[Weather] masterTable �� ����ֽ��ϴ�. SO�� ����Ʈ�� ����ϼ���.");
            return;
        }

        /* 1) ��� ���� ���͸� */
        var list = GetUnlockedWeathers(GameStateData.I.currentDay);
        if (list.Count == 0)
        {
            Debug.LogWarning("[Weather] ���� ���൵���� ���� ������ ������ �����ϴ�.");
            return;
        }

        /* 2) Ȯ�� ���� ���� */
        today = GetRandomWeighted(list);

        /* 3) GameStateData �� ��� */
        GameStateData.I.SetWeather(masterTable.IndexOf(today));

        /* 4) ȿ�� ���� & �̺�Ʈ */
        ApplyWeatherEffects(today);
        OnWeatherChanged?.Invoke(today);

        Debug.Log($"[Weather] �� {today.weatherName}");
    }

    /* ������������������ ���� �Լ� ������������������ */
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

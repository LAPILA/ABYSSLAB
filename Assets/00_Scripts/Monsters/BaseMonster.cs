using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BaseMonster : MonoBehaviour
{
    public int MonsterID;
    public float StressLevel;
    public bool IsHostile;
    public float HearingRange;
    public float SightRange;
    public bool CanEvolve;

    public event Action<int> OnEscape;
    public event Action<int, float> OnStressChanged;

    private void Update()
    {
        if (StressLevel >= 100)
        {
            OnEscape?.Invoke(MonsterID);
        }
    }

    public void TakeDamage(float damage)
    {
        StressLevel += damage;
        OnStressChanged?.Invoke(MonsterID, StressLevel);
    }

    public void Calmdown()
    {
        StressLevel = Mathf.Max(0, StressLevel - 20);
        OnStressChanged?.Invoke(MonsterID, StressLevel);
    }
}

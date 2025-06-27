using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class WeaperController : BaseMonster, IFeedReceiver
{
    /*─────────────────────────────────────────────────────────────
     * ① 필드 & 컴포넌트
     *────────────────────────────────────────────────────────────*/
    [InlineEditor, Required, SerializeField]
    private WeaperData data;

    [BoxGroup("Runtime"), ShowInInspector, ReadOnly] private float hungry;
    [BoxGroup("Runtime"), ShowInInspector, ReadOnly] private bool isCleaning;

    private NavMeshAgent agent;
    private float decalScanTimer;

    /*─────────────────────────────────────────────────────────────
     * ② 초기화
     *────────────────────────────────────────────────────────────*/
    protected override void Awake()
    {
        base.Awake();                       // BaseMonster 내부 Init
        agent = GetComponent<NavMeshAgent>();

        // 베이스 Stat 세팅
        InitStats(data.maxHP, data.maxStress);
        hungry = data.maxHungry;
        SightRange = data.sightRange;
        HearingRange = data.hearingRange;
        Disposition = MonsterDisposition.Friendly;
    }

    /*─────────────────────────────────────────────────────────────
     * ③ 메인 Loop
     *────────────────────────────────────────────────────────────*/
    private void Update()
    {
        base.CommonTick();                  // 스트레스·HP 자연 감소 등

        // 허기 감소 → Hungry 상태
        hungry -= data.hungryDrainPerSec * Time.deltaTime;
        if (hungry < 0) hungry = 0;

        // 3 초마다 오염 데칼 스캔
        decalScanTimer += Time.deltaTime;
        if (decalScanTimer >= 3f && !isCleaning)
        {
            decalScanTimer = 0f;
        }

        // 스트레스 80% 이상 → 구토
        if (StressNormalized >= 0.8f && !IsDead)
        {
        }
    }

}

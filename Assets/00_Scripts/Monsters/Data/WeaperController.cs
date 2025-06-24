using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class WeaperController : BaseMonster, IFeedReceiver
{
    /*��������������������������������������������������������������������������������������������������������������������������
     * �� �ʵ� & ������Ʈ
     *������������������������������������������������������������������������������������������������������������������������*/
    [InlineEditor, Required, SerializeField]
    private WeaperData data;

    [BoxGroup("Runtime"), ShowInInspector, ReadOnly] private float hungry;
    [BoxGroup("Runtime"), ShowInInspector, ReadOnly] private bool isCleaning;

    private NavMeshAgent agent;
    private float decalScanTimer;

    /*��������������������������������������������������������������������������������������������������������������������������
     * �� �ʱ�ȭ
     *������������������������������������������������������������������������������������������������������������������������*/
    protected override void Awake()
    {
        base.Awake();                       // BaseMonster ���� Init
        agent = GetComponent<NavMeshAgent>();

        // ���̽� Stat ����
        InitStats(data.maxHP, data.maxStress);
        hungry = data.maxHungry;
        SightRange = data.sightRange;
        HearingRange = data.hearingRange;
        Disposition = MonsterDisposition.Friendly;
    }

    /*��������������������������������������������������������������������������������������������������������������������������
     * �� ���� Loop
     *������������������������������������������������������������������������������������������������������������������������*/
    private void Update()
    {
        base.CommonTick();                  // ��Ʈ������HP �ڿ� ���� ��

        // ��� ���� �� Hungry ����
        hungry -= data.hungryDrainPerSec * Time.deltaTime;
        if (hungry < 0) hungry = 0;

        // 3 �ʸ��� ���� ��Į ��ĵ
        decalScanTimer += Time.deltaTime;
        if (decalScanTimer >= 3f && !isCleaning)
        {
            decalScanTimer = 0f;
        }

        // ��Ʈ���� 80% �̻� �� ����
        if (StressNormalized >= 0.8f && !IsDead)
        {
        }
    }

}

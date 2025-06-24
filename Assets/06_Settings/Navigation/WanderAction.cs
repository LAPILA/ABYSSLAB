using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Wander", story: "[Self] Navigate to WanderPosition", category: "Action", id: "ce01dadcb50fa23797dfe36caf2ebcb0")]

public partial class WanderAction : Action
{
    /* ──────────────── Blackboard Links ──────────────── */
    // 반드시 "Self" 에 NavMeshAgent 가진 GameObject 를 주입해야 합니다.
    [SerializeReference, Tooltip("Agent GameObject")]
    public BlackboardVariable<GameObject> Self = new BlackboardVariable<GameObject>();

    /* ──────────────── Tunables ──────────────── */
    [SerializeField, Tooltip("한 번에 이동할 최대 반경")]
    private float wanderRadius = 4.0f;

    [SerializeField, Tooltip("목표 지점 도착 판정 거리")]
    private float arriveThreshold = 0.3f;

    [SerializeField, Tooltip("Destinoation 을 다시 고를 최소~최대 대기 시간(sec)")]
    private Vector2 repathDelayRange = new Vector2(1.5f, 3.0f);

    /* ──────────────── Runtime ──────────────── */
    private NavMeshAgent _agent;
    private float _nextRepathAt;
    private System.Random _rng;    // deterministic 하게 쓰고 싶으면 seed 전달

    /* ===================================================================== */
    /* 1) OnStart : 초기 셋업                                                */
    /* ===================================================================== */
    protected override Status OnStart()
    {
        var go = Self?.Value;
        if (go == null)
        {
            Debug.LogError("[WanderAction] Self GameObject 가 없습니다.");
            return Status.Failure;
        }

        _agent = go.GetComponent<NavMeshAgent>();
        if (_agent == null)
        {
            Debug.LogError("[WanderAction] NavMeshAgent 가 없습니다.");
            return Status.Failure;
        }

        _rng = _rng ?? new System.Random();
        PickNewDestination();

        return Status.Running;
    }

    /* ===================================================================== */
    /* 2) OnUpdate : 매 Tick 호출                                            */
    /* ===================================================================== */
    protected override Status OnUpdate()
    {
        if (_agent == null) return Status.Failure;
        if (!_agent.enabled || !_agent.isOnNavMesh) return Status.Failure;

        // ① 도착했거나 ② 재-Path 시간이 됐으면 새 목표
        bool arrived = !_agent.pathPending && _agent.remainingDistance <= arriveThreshold;
        bool needRepath = Time.time >= _nextRepathAt;

        if (arrived || needRepath)
        {
            PickNewDestination();
        }

        // Wander 는 무한 지속 – 항상 Running 반환
        return Status.Running;
    }

    /* ===================================================================== */
    /* 3) OnEnd : 클린업(필요하면)                                           */
    /* ===================================================================== */
    protected override void OnEnd()
    {
        // 특별한 정리 없음.  Stop 시켜두고 싶다면:
        // if (_agent && _agent.isOnNavMesh) _agent.ResetPath();
    }

    /* ===================================================================== */
    /* ■ Helper : 새 목적지 선택                                             */
    /* ===================================================================== */
    private void PickNewDestination()
    {
        if (_agent == null || !_agent.isOnNavMesh) return;

        Vector3 origin = _agent.transform.position;
        Vector3 randomDir =
            new Vector3(RandomRangeSigned(), 0, RandomRangeSigned()).normalized * wanderRadius;

        Vector3 dest = origin + randomDir;

        // NavMesh 위의 가깝고 유효한 포인트 샘플
        if (NavMesh.SamplePosition(dest, out var hit, wanderRadius, NavMesh.AllAreas))
        {
            _agent.SetDestination(hit.position);
        }

        // 다음 re-path 시각 설정
        float delay = Mathf.Lerp(repathDelayRange.x, repathDelayRange.y,
                                 (float)_rng.NextDouble());
        _nextRepathAt = Time.time + delay;
    }

    private float RandomRangeSigned()
    {
        // -1.0f ~ 1.0f 균등
        return (float)_rng.NextDouble() * 2f - 1f;
    }
}

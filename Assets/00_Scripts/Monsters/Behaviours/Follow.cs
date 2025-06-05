using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : BehaviourNode
{
    [SerializeField]
    public Transform target;

    public override IEnumerator Visit()
    {
        status = BehaviourNodeStatus.Ready;

        // 걷기 지속시간
        const float time = 0.5f;

        // 이동 방향을 타겟과 방향으로 설정
        Vector3 dist = target.position - transform.position;
        dist = new Vector3(dist.x, 0.0f, dist.z);

        Vector3 direction = dist.normalized;
        transform.forward = direction;

        // 시작 위치, 도착 위치 설정
        Vector3 startingPos = transform.position;
        Vector3 finalPos = transform.position + (transform.forward * 1.0f);

        float elapsedTime = 0;

        // avoid 시작
        status = BehaviourNodeStatus.Running;
        while (elapsedTime < time) 
        {/*
            // 만약 벽과 너무 가까우면, 정지
            Debug.DrawLine(transform.position, transform.position + transform.forward * 0.5f + Vector3.up * 0.02f);
            if (Physics.Raycast(new Ray(transform.position + Vector3.up * 0.02f, transform.forward), 0.5f))
            {
                break;
            }*/

            transform.position = Vector3.Lerp(startingPos, finalPos, (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 마치기
        status = BehaviourNodeStatus.Success;
        yield return null;
    }
}

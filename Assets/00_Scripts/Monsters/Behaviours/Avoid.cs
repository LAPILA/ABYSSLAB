using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Avoid : BehaviourNode
{
    [SerializeField]
    public Transform target;

    [SerializeField]
    public const int maxTries = 10;

    public override IEnumerator Visit()
    {
        status = BehaviourNodeStatus.Ready;

        // 걷기 지속시간
        const float time = 2.0f;

        // 이동 방향을 타겟과 반대로 설정
        Vector3 dist = transform.position - target.position;
        dist = new Vector3(dist.x, 0.0f, dist.z);

        // 랜덤 범위 설정 시도 (60도) 
        Vector3 direction = dist.normalized;
        int i = 0;
        for (i=0;i<maxTries;i++)
        {
            direction = Quaternion.Euler(0.0f, Random.Range(-90.0f, 90.0f), 0.0f) * dist.normalized;
            if (!Physics.Raycast(new Ray(transform.position, direction), 0.5f))
                break;
        }

        if (i == maxTries)
        {
            status = BehaviourNodeStatus.Running;
            yield return new WaitForSeconds(0.5f);
        }
            
        transform.forward = direction;

        // 시작 위치, 도착 위치 설정
        Vector3 startingPos = transform.position;
        Vector3 finalPos = transform.position + (transform.forward * 5);

        float elapsedTime = 0;

        // avoid 시작
        status = BehaviourNodeStatus.Running;
        while (elapsedTime < time) 
        {
            // 만약 벽과 너무 가까우면, 정지
            Debug.DrawLine(transform.position, transform.position + transform.forward * 0.5f + Vector3.up * 0.02f);
            if (Physics.Raycast(new Ray(transform.position + Vector3.up * 0.02f, transform.forward), 0.5f))
            {
                break;
            }

            transform.position = Vector3.Lerp(startingPos, finalPos, (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 마치기
        status = BehaviourNodeStatus.Success;
        yield return null;
    }
}

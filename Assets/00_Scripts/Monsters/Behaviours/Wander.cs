using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wander : BehaviourNode
{

    public override IEnumerator Visit()
    {
        status = BehaviourNodeStatus.Ready;

        // 걷기 지속시간
        const float time = 2.0f;

        // 이동 방향을 랜덤하게 설정
        Vector3 angle = new Vector3(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
        transform.eulerAngles = angle;

        // 시작 위치, 도착 위치 설정
        Vector3 startingPos = transform.position;
        Vector3 finalPos = transform.position + (transform.forward * 5);

        float elapsedTime = 0;

        // wander 시작
        status = BehaviourNodeStatus.Running;
        while (elapsedTime < time)
        {
            // 만약 벽과 너무 가까우면, 정지
            Debug.DrawLine(transform.position + Vector3.up * 0.02f, transform.position + transform.forward * 0.5f + Vector3.up * 0.02f);
            if (Physics.Raycast(new Ray(transform.position + Vector3.up * 0.02f, transform.forward), 0.5f))
            {
                break;
            }

            transform.position = Vector3.Lerp(startingPos, finalPos, (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 1초 휴식
        yield return new WaitForSeconds(1);

        // 마치기
        status = BehaviourNodeStatus.Success;
        yield return null;
    }
}

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

        // �ȱ� ���ӽð�
        const float time = 0.5f;

        // �̵� ������ Ÿ�ٰ� �������� ����
        Vector3 dist = target.position - transform.position;
        dist = new Vector3(dist.x, 0.0f, dist.z);

        Vector3 direction = dist.normalized;
        transform.forward = direction;

        // ���� ��ġ, ���� ��ġ ����
        Vector3 startingPos = transform.position;
        Vector3 finalPos = transform.position + (transform.forward * 1.0f);

        float elapsedTime = 0;

        // avoid ����
        status = BehaviourNodeStatus.Running;
        while (elapsedTime < time) 
        {/*
            // ���� ���� �ʹ� ������, ����
            Debug.DrawLine(transform.position, transform.position + transform.forward * 0.5f + Vector3.up * 0.02f);
            if (Physics.Raycast(new Ray(transform.position + Vector3.up * 0.02f, transform.forward), 0.5f))
            {
                break;
            }*/

            transform.position = Vector3.Lerp(startingPos, finalPos, (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // ��ġ��
        status = BehaviourNodeStatus.Success;
        yield return null;
    }
}

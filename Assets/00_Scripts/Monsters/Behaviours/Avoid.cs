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

        // �ȱ� ���ӽð�
        const float time = 2.0f;

        // �̵� ������ Ÿ�ٰ� �ݴ�� ����
        Vector3 dist = transform.position - target.position;
        dist = new Vector3(dist.x, 0.0f, dist.z);

        // ���� ���� ���� �õ� (60��) 
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

        // ���� ��ġ, ���� ��ġ ����
        Vector3 startingPos = transform.position;
        Vector3 finalPos = transform.position + (transform.forward * 5);

        float elapsedTime = 0;

        // avoid ����
        status = BehaviourNodeStatus.Running;
        while (elapsedTime < time) 
        {
            // ���� ���� �ʹ� ������, ����
            Debug.DrawLine(transform.position, transform.position + transform.forward * 0.5f + Vector3.up * 0.02f);
            if (Physics.Raycast(new Ray(transform.position + Vector3.up * 0.02f, transform.forward), 0.5f))
            {
                break;
            }

            transform.position = Vector3.Lerp(startingPos, finalPos, (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // ��ġ��
        status = BehaviourNodeStatus.Success;
        yield return null;
    }
}

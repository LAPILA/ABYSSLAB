using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wander : BehaviourNode
{

    public override IEnumerator Visit()
    {
        status = BehaviourNodeStatus.Ready;

        // �ȱ� ���ӽð�
        const float time = 2.0f;

        // �̵� ������ �����ϰ� ����
        Vector3 angle = new Vector3(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
        transform.eulerAngles = angle;

        // ���� ��ġ, ���� ��ġ ����
        Vector3 startingPos = transform.position;
        Vector3 finalPos = transform.position + (transform.forward * 5);

        float elapsedTime = 0;

        // wander ����
        status = BehaviourNodeStatus.Running;
        while (elapsedTime < time)
        {
            // ���� ���� �ʹ� ������, ����
            Debug.DrawLine(transform.position + Vector3.up * 0.02f, transform.position + transform.forward * 0.5f + Vector3.up * 0.02f);
            if (Physics.Raycast(new Ray(transform.position + Vector3.up * 0.02f, transform.forward), 0.5f))
            {
                break;
            }

            transform.position = Vector3.Lerp(startingPos, finalPos, (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 1�� �޽�
        yield return new WaitForSeconds(1);

        // ��ġ��
        status = BehaviourNodeStatus.Success;
        yield return null;
    }
}

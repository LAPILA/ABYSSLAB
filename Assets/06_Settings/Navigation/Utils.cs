// ��������������������������������������������������������������������������������������������������������������������������
// Utils.cs - 3D�� ���� �Լ� (x-z ��� ���� ��ǥ)
// ��������������������������������������������������������������������������������������������������������������������������
using UnityEngine;
using UnityEngine.AI;

public static class Utils
{
    /// <summary>
    /// (xz ���) �߽��� + �ݰ桤������ ���� ��ǥ ���
    /// </summary>
    /// <param name="center">world center</param>
    /// <param name="radius">R</param>
    /// <param name="degree">Degree (0-360)</param>
    public static Vector3 GetXZCirclePos(Vector3 center, float radius, float degree)
    {
        float rad = degree * Mathf.Deg2Rad;
        return new Vector3(
            center.x + Mathf.Cos(rad) * radius,
            center.y,                                   // y ����
            center.z + Mathf.Sin(rad) * radius
        );
    }

    /// <summary> NavMesh ���� ��ȿ ����Ʈ�� ��ȯ (���� �� false) </summary>
    public static bool TryGetNavMeshPos(Vector3 src, out Vector3 hitPos, float maxDist = 2f, int areaMask = NavMesh.AllAreas)
    {
        if (NavMesh.SamplePosition(src, out var hit, maxDist, areaMask))
        {
            hitPos = hit.position;
            return true;
        }
        hitPos = src;
        return false;
    }
}

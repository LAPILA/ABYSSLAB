// ─────────────────────────────────────────────────────────────
// Utils.cs - 3D용 도움 함수 (x-z 평면 원형 좌표)
// ─────────────────────────────────────────────────────────────
using UnityEngine;
using UnityEngine.AI;

public static class Utils
{
    /// <summary>
    /// (xz 평면) 중심점 + 반경·각도로 원주 좌표 계산
    /// </summary>
    /// <param name="center">world center</param>
    /// <param name="radius">R</param>
    /// <param name="degree">Degree (0-360)</param>
    public static Vector3 GetXZCirclePos(Vector3 center, float radius, float degree)
    {
        float rad = degree * Mathf.Deg2Rad;
        return new Vector3(
            center.x + Mathf.Cos(rad) * radius,
            center.y,                                   // y 고정
            center.z + Mathf.Sin(rad) * radius
        );
    }

    /// <summary> NavMesh 상의 유효 포인트를 반환 (실패 시 false) </summary>
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

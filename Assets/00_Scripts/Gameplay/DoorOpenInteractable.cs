using UnityEngine;
using DG.Tweening;

public class DoorOpenInteractable : BaseInteractable
{
    [Header("Indicator Mesh (BlendShapes)")]
    [SerializeField] private SkinnedMeshRenderer indicatorMesh;
    [SerializeField] private string greenBlend = "Green_Light";
    [SerializeField] private string redBlend = "Red_Light";

    [Header("Lever")]
    [SerializeField] private Transform leverHandle;
    [SerializeField] private float leverDownAngle = 180f;
    [SerializeField] private float leverMoveTime = 0.3f;

    // ───── 내부
    private int greenIdx = -1, redIdx = -1;
    private bool isOpen = true;
    private bool isAnimating = false;
    private Sequence seq;

    void Awake()
    {
        if (indicatorMesh != null)
        {
            var m = indicatorMesh.sharedMesh;
            greenIdx = m.GetBlendShapeIndex(greenBlend);
            redIdx = m.GetBlendShapeIndex(redBlend);
        }
        ApplyLight();          // 초기 상태
        ApplyLeverImmediate();
    }

    public override void Interact()
    {
        if (isAnimating) return;
        ToggleDoor();
    }

    /* ──────────── Core ──────────── */

    void ToggleDoor()
    {
        isAnimating = true;
        bool targetOpen = !isOpen;

        seq?.Kill();

        float targetX = targetOpen ? 0f : leverDownAngle;

        // 레버 회전(Quaternion) + OnComplete BlendShape 전환
        seq = DOTween.Sequence()
            .Append(
                leverHandle
                    .DOLocalRotate(
                        new Vector3(targetX, 0f, 0f),
                        leverMoveTime, RotateMode.Fast
                    )
            )
            .AppendCallback(() =>
            {
                isOpen = targetOpen;
                ApplyLight();
                isAnimating = false;
            });
    }

    /* ──────────── Helpers ──────────── */

    void ApplyLight()
    {
        if (indicatorMesh == null) return;
        if (greenIdx >= 0) indicatorMesh.SetBlendShapeWeight(greenIdx, isOpen ? 100 : 0);
        if (redIdx >= 0) indicatorMesh.SetBlendShapeWeight(redIdx, isOpen ? 0 : 100);
    }

    void ApplyLeverImmediate()
    {
        if (leverHandle == null) return;
        Vector3 eul = leverHandle.localEulerAngles;
        eul.x = isOpen ? 0f : leverDownAngle;
        leverHandle.localEulerAngles = eul;
    }
}

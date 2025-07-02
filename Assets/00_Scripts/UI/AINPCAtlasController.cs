using UnityEngine;
using Spine;
using Spine.Unity;

/// <summary>
/// SkeletonGraphic(AINPC Atlas) �ִϸ��̼� ��Ʈ�ѷ�
/// - ���� : enter ������ 0���� Freeze
/// - Key O :    enter 1ȸ �� idle ����
/// </summary>
[RequireComponent(typeof(SkeletonGraphic))]
public class AINPCAtlasGraphicController : MonoBehaviour
{
    [Header("Spine Graphic")]
    [SerializeField] SkeletonGraphic skelGraphic;

    [SpineAnimation][SerializeField] string enterAnim = "enter";
    [SpineAnimation][SerializeField] string idleAnim = "idle";

    [Header("Debug Key")]
    [SerializeField] KeyCode debugKey = KeyCode.O;

    /* ���� */
    bool frozen = true;
    TrackEntry running;

    void Awake()
    {
        if (!skelGraphic) skelGraphic = GetComponent<SkeletonGraphic>();

        // SkeletonGraphic �� Initialize() �� �̹� ȣ��� ���� (Canvas ���� ��)
        running = skelGraphic.AnimationState.SetAnimation(0, enterAnim, false);
        skelGraphic.timeScale = 0f;     // Freeze
        frozen = true;
    }

    void Update()
    {
        if (Input.GetKeyDown(debugKey))
            PlaySequence();
    }

    /* ------------------------------------------------------------------ */
    void PlaySequence()
    {
        if (!frozen) return;

        frozen = false;
        skelGraphic.timeScale = 1f;     // ����

        running = skelGraphic.AnimationState.SetAnimation(0, enterAnim, false);
        running.Complete += OnEnterComplete;   // �ߺ� �ڵ� ����(Spine �̺�Ʈ���� ����)
    }

    void OnEnterComplete(TrackEntry track)
    {
        // enter �� �� idle(loop)
        skelGraphic.AnimationState.SetAnimation(0, idleAnim, true);
        running = null;
        Debug.Log("[AINPC] enter �� idle(loop) �Ϸ�");
    }
}

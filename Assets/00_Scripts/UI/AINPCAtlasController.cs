using UnityEngine;
using Spine;
using Spine.Unity;

/// <summary>
/// SkeletonGraphic(AINPC Atlas) 애니메이션 컨트롤러
/// - 시작 : enter 프레임 0에서 Freeze
/// - Key O :    enter 1회 ▶ idle 루프
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

    /* 내부 */
    bool frozen = true;
    TrackEntry running;

    void Awake()
    {
        if (!skelGraphic) skelGraphic = GetComponent<SkeletonGraphic>();

        // SkeletonGraphic 은 Initialize() 가 이미 호출돼 있음 (Canvas 렌더 시)
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
        skelGraphic.timeScale = 1f;     // 해제

        running = skelGraphic.AnimationState.SetAnimation(0, enterAnim, false);
        running.Complete += OnEnterComplete;   // 중복 자동 방지(Spine 이벤트마다 제거)
    }

    void OnEnterComplete(TrackEntry track)
    {
        // enter 끝 → idle(loop)
        skelGraphic.AnimationState.SetAnimation(0, idleAnim, true);
        running = null;
        Debug.Log("[AINPC] enter ▶ idle(loop) 완료");
    }
}

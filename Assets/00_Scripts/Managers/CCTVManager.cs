//==============================================================================
// CCTV 시스템 총괄 매니저 ― FIXED v4 (Global Angle Limits)
// - 모든 카메라가 동일한 Yaw/Pitch 제한을 공유하도록 변경
// - Slot별 yawLimit / pitchLimit 필드 제거
// - 추후 "카메라 락" 기믹(몬스터 효과)용 플래그만 자리 마련
//==============================================================================

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using Unity.Cinemachine;

public class CCTVManager : MonoBehaviour
{
    // ────────────────── 카메라 슬롯 구조 ──────────────────
    [System.Serializable]
    public class CCTVSlot
    {
        [HorizontalGroup("Split", Width = 60), LabelText("이름"), Required]
        public string cameraName;

        [HorizontalGroup("Split"), Required]
        public Camera cctvCamera;

        [HorizontalGroup("Split"), Required]
        public RenderTexture renderTexture;

        [FoldoutGroup("Room"), LabelText("연결 Room")] public GameObject roomRef;

        [FoldoutGroup("Room"), LabelText("회전 Pivot (선택)")]
        public Transform pivot; // 회전 적용 트랜스폼 (null → 카메라 Transform)

        /// <summary>실제 회전에 사용될 Transform 반환</summary>
        public Transform GetPivot() => pivot ? pivot : cctvCamera.transform;
    }

    // ────────────────── 인스펙터 세팅 ──────────────────
    [TitleGroup("CCTV Settings"), ListDrawerSettings]
    [SerializeField, Required] private List<CCTVSlot> cctvList = new();

    [TitleGroup("UI & View"), Required]
    [SerializeField] private RawImage mainCCTVScreen;

    [TitleGroup("UI & View")]
    [SerializeField] private CanvasGroup fadeEffect;

    [TitleGroup("Player & Cameras")]
    [SerializeField] private FirstPersonController playerController;
    [SerializeField] private CinemachineCamera playerCam;  // 플레이어 1인칭 카메라
    [SerializeField] private CinemachineCamera overlayCam; // CCTV 뷰 UI용 (HUD 등)

    [TitleGroup("Limit Settings"), LabelText("Yaw 제한 (deg)")]
    [MinMaxSlider(-180f, 180f)] public Vector2 yawLimit = new(-90f, 90f);

    [TitleGroup("Limit Settings"), LabelText("Pitch 제한 (deg)")]
    [MinMaxSlider(-90f, 90f)] public Vector2 pitchLimit = new(-30f, 30f);

    [TitleGroup("Runtime"), ShowInInspector, ReadOnly]
    private int currentCameraIndex = -1;

    [SerializeField] private bool isViewing = false;

    [SerializeField, Min(1f), Tooltip("마우스 1px당 회전 속도 (deg/sec)")]
    private float rotationSpeed = 60f;

    [SerializeField, Tooltip("마우스 Y 반전 여부")]
    private bool invertY = false;

    // 추후 몬스터 효과: 카메라 고정 플래그 (현재는 항상 false)
    private bool isCameraLocked = false;

    private float cctvYaw = 0f;   // 누적 값 (deg)
    private float cctvPitch = 0f; // 누적 값 (deg)

    public Action<int, string> OnCameraSwitched;   // (index, name)

    // ────────────────── 초기화 ──────────────────
    private void Awake()
    {
        if (cctvList == null || cctvList.Count == 0)
        {
            Debug.LogError("[CCTVManager] CCTV List is empty!");
            enabled = false;
            return;
        }

        foreach (var slot in cctvList)
        {
            if (slot.cctvCamera) slot.cctvCamera.enabled = false;
        }

        if (overlayCam) overlayCam.gameObject.SetActive(false);
    }

    private void Start() => SwitchToCameraByIndex(0);

    // ────────────────── Public API ──────────────────
    public void SwitchToCameraByIndex(int idx)
    {
        if (idx < 0 || idx >= cctvList.Count || idx == currentCameraIndex) return;
        SetActiveCamera(idx);
    }
    public void SwitchNextCamera() => SetActiveCamera((currentCameraIndex + 1) % cctvList.Count);
    public void SwitchPrevCamera() => SetActiveCamera((currentCameraIndex - 1 + cctvList.Count) % cctvList.Count);

    public void StartCCTVView()
    {
        if (isViewing) return;

        isViewing = true;
        ResetRotation();

        if (playerCam) playerCam.gameObject.SetActive(false);
        if (overlayCam) overlayCam.gameObject.SetActive(true);

        if (playerController)
        {
            playerController.StopMove();
            if (playerController.playerInput) playerController.playerInput.enabled = false;
        }
    }

    public void EndCCTVView()
    {
        if (!isViewing) return;
        isViewing = false;

        if (playerCam) playerCam.gameObject.SetActive(true);
        if (overlayCam) overlayCam.gameObject.SetActive(false);

        if (playerController)
        {
            playerController.StartMove();
            if (playerController.playerInput) playerController.playerInput.enabled = true;
        }
    }

    // ────────────────── 내부 로직 ──────────────────
    private void SetActiveCamera(int idx, bool instantFade = false)
    {
        if (currentCameraIndex >= 0 && currentCameraIndex < cctvList.Count)
        {
            var prevCam = cctvList[currentCameraIndex].cctvCamera;
            if (prevCam) prevCam.enabled = false;
        }

        currentCameraIndex = idx;

        var slot = cctvList[idx];
        if (slot.cctvCamera) slot.cctvCamera.enabled = true;
        if (mainCCTVScreen) mainCCTVScreen.texture = slot.renderTexture;

        // TODO: 추후 페이드 인아웃 효과 넣기

        ResetRotation();
        OnCameraSwitched?.Invoke(idx, slot.cameraName);
    }

    private void ResetRotation()
    {
        cctvYaw = Mathf.Clamp(0f, yawLimit.x, yawLimit.y);
        cctvPitch = Mathf.Clamp(0f, pitchLimit.x, pitchLimit.y);
        var pivot = GetCurrentPivot();
        if (pivot) pivot.localRotation = Quaternion.identity;
    }

    private Transform GetCurrentPivot() =>
        (currentCameraIndex >= 0 && currentCameraIndex < cctvList.Count) ? cctvList[currentCameraIndex].GetPivot() : null;

    // ────────────────── 런타임 입력 처리 ──────────────────
#if UNITY_EDITOR
    private void Update()
    {
        // 테스트 입력: Q/E 전환, T 토글
        if (Input.GetKeyDown(KeyCode.Q)) SwitchPrevCamera();
        if (Input.GetKeyDown(KeyCode.E)) SwitchNextCamera();
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (isViewing) EndCCTVView();
            else StartCCTVView();
        }

        if (isViewing && !isCameraLocked && currentCameraIndex >= 0)
            HandleMouseLook();
    }
#endif

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        if (Mathf.Approximately(mouseX, 0f) && Mathf.Approximately(mouseY, 0f)) return;

        // 1. 누적값 계산
        cctvYaw += mouseX * rotationSpeed * Time.deltaTime;
        cctvPitch += (invertY ? mouseY : -mouseY) * rotationSpeed * Time.deltaTime;

        // 2. 글로벌 제한 적용
        cctvYaw = Mathf.Clamp(cctvYaw, yawLimit.x, yawLimit.y);
        cctvPitch = Mathf.Clamp(cctvPitch, pitchLimit.x, pitchLimit.y);

        // 3. 적용
        var pivot = GetCurrentPivot();
        if (pivot) pivot.localRotation = Quaternion.Euler(cctvPitch, cctvYaw, 0f);
    }
}

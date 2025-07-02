//==============================================================================
// CCTVManager (맵 패널 연동, 좌클릭 줌, 상태/수리/디버그, Info 미리보기)
//==============================================================================

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using Unity.Cinemachine;

public enum CameraState { Active, Off, Broken }

public class CCTVManager : MonoBehaviour
{
    // ────────────── CCTV 슬롯 구조 ──────────────
    [System.Serializable]
    public class CCTVSlot
    {
        [HorizontalGroup("Split"), LabelText("이름"), Required] public string cameraName;
        [HorizontalGroup("Split"), Required] public Camera cctvCamera;
        [HorizontalGroup("Split"), Required] public RenderTexture renderTexture;
        [FoldoutGroup("Room"), LabelText("연결 Room")] public GameObject roomRef;
        [FoldoutGroup("Room"), LabelText("회전 Pivot (선택)")] public Transform pivot;
        [FoldoutGroup("Option"), LabelText("파괴 가능")] public bool destructible = true;
        [FoldoutGroup("Option"), LabelText("내구도")] public int durability = 100;
        [ReadOnly, FoldoutGroup("Runtime")] public CameraState cameraState = CameraState.Active;

        public Transform GetPivot() => pivot ? pivot : cctvCamera.transform;

        public void CheckAndUpdateState()
        {
            if (destructible && durability <= 0)
                cameraState = CameraState.Broken;
            if (!destructible) durability = 100;
        }
        public void Repair()
        {
            if (!destructible) return;
            durability = 100;
            cameraState = CameraState.Active;
        }
    }

    // ────────────── 인스펙터 세팅 ──────────────
    [TitleGroup("CCTV Settings"), ListDrawerSettings]
    [SerializeField, Required] private List<CCTVSlot> cctvList = new();

    [TitleGroup("UI & View"), Required][SerializeField] private RawImage mainCCTVScreen;
    [TitleGroup("UI & View")][SerializeField] private CanvasGroup fadeEffect;
    [TitleGroup("UI & View"), Required][SerializeField] private Text camName;
    [TitleGroup("UI & View"), Required][SerializeField] private GameObject brokeIcon;
    [TitleGroup("UI & View"), Required][SerializeField] private GameObject offIcon;

    [TitleGroup("Panel References"), Required]
    [SerializeField] private GameObject CCTVViewPanel;
    [TitleGroup("Panel References"), Required]
    [SerializeField] private GameObject MinimapPanel;
    [TitleGroup("Panel References")]
    [SerializeField] private MapPanelManager mapPanelManager;

    [TitleGroup("Player & Cameras")]
    [SerializeField] private FirstPersonController playerController;
    [SerializeField] private CinemachineCamera playerCam;
    [SerializeField] private CinemachineCamera overlayCam;

    [TitleGroup("Limit Settings"), LabelText("Yaw 제한 (deg)")]
    [MinMaxSlider(-180f, 180f)] public Vector2 yawLimit = new(-90f, 90f);
    [TitleGroup("Limit Settings"), LabelText("Pitch 제한 (deg)")]
    [MinMaxSlider(-90f, 90f)] public Vector2 pitchLimit = new(-30f, 30f);

    // ────────────── 런타임 변수 ──────────────
    [TitleGroup("Runtime"), ShowInInspector, ReadOnly]
    private int currentCameraIndex = -1;
    [SerializeField] private bool isViewing = false;
    [SerializeField, Min(1f)] private float rotationSpeed = 60f;
    [SerializeField] private bool invertY = false;
    private bool isCameraLocked = false;
    private float cctvYaw = 0f;
    private float cctvPitch = 0f;

    // FOV(줌) 변수
    [Header("Zoom Settings")]
    [SerializeField] private float defaultFOV = 60f;
    [SerializeField] private float zoomedFOV = 30f;
    [SerializeField] private float zoomLerpTime = 0.15f;
    private Coroutine zoomCoroutine;

    public Action<int, string> OnCameraSwitched;

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
            if (slot.cctvCamera)
            {
                slot.cctvCamera.enabled = false;
                slot.cctvCamera.fieldOfView = defaultFOV;
            }
        }
        if (overlayCam) overlayCam.gameObject.SetActive(false);
        if (brokeIcon) brokeIcon.SetActive(false);
        if (offIcon) offIcon.SetActive(false);

        if (CCTVViewPanel) CCTVViewPanel.SetActive(true);
        if (MinimapPanel) MinimapPanel.SetActive(false);
    }

    private void Start() => SwitchToCameraByIndex(0);

    // ──────── CCTV 전환/제어 ────────
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
        if (CCTVViewPanel) CCTVViewPanel.SetActive(true);
        if (MinimapPanel) MinimapPanel.SetActive(false);
        if (playerController)
        {
            playerController.StopMove();
            if (playerController.playerInput) playerController.playerInput.enabled = false;
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void EndCCTVView()
    {
        if (!isViewing) return;
        isViewing = false;
        if (playerCam) playerCam.gameObject.SetActive(true);
        if (overlayCam) overlayCam.gameObject.SetActive(false);
        if (CCTVViewPanel) CCTVViewPanel.SetActive(false);
        if (playerController)
        {
            playerController.StartMove();
            if (playerController.playerInput) playerController.playerInput.enabled = true;
        }
    }

    // 내부 전환/상태 갱신
    private void SetActiveCamera(int idx, bool instantFade = false)
    {
        if (currentCameraIndex >= 0 && currentCameraIndex < cctvList.Count)
        {
            var prevCam = cctvList[currentCameraIndex].cctvCamera;
            if (prevCam)
            {
                prevCam.enabled = false;
                prevCam.fieldOfView = defaultFOV;
            }
        }
        currentCameraIndex = idx;
        var slot = cctvList[idx];
        if (slot.cctvCamera)
        {
            slot.cctvCamera.enabled = true;
            slot.cctvCamera.fieldOfView = defaultFOV;
        }
        if (mainCCTVScreen) mainCCTVScreen.texture = slot.renderTexture;
        ResetRotation();
        UpdateCCTVUI();
        OnCameraSwitched?.Invoke(idx, slot.cameraName);
    }

    private void UpdateCCTVUI()
    {
        var slot = GetCurrentSlot();
        if (camName) camName.text = slot.cameraName;
        slot.CheckAndUpdateState();
        bool isBroken = slot.cameraState == CameraState.Broken;
        bool isOff = slot.cameraState == CameraState.Off;
        if (brokeIcon) brokeIcon.SetActive(isBroken);
        if (offIcon) offIcon.SetActive(isOff || isBroken);
    }

    // 상태/내구도/수리 외부 인터페이스
    public void SetCameraOff(int idx, bool isOff)
    {
        if (idx < 0 || idx >= cctvList.Count) return;
        var slot = cctvList[idx];
        if (slot.cameraState == CameraState.Broken) return;
        slot.cameraState = isOff ? CameraState.Off : CameraState.Active;
        if (idx == currentCameraIndex) UpdateCCTVUI();
    }
    public void DamageCamera(int idx, int damage)
    {
        if (idx < 0 || idx >= cctvList.Count) return;
        var slot = cctvList[idx];
        if (!slot.destructible) return;
        slot.durability -= damage;
        slot.CheckAndUpdateState();
        if (idx == currentCameraIndex) UpdateCCTVUI();
    }
    public void RepairCamera(int idx)
    {
        if (idx < 0 || idx >= cctvList.Count) return;
        var slot = cctvList[idx];
        slot.Repair();
        if (idx == currentCameraIndex) UpdateCCTVUI();
    }

    // 맵 패널 진입/복귀
    public void ShowMinimapPanel()
    {
        if (MinimapPanel) MinimapPanel.SetActive(true);
        if (CCTVViewPanel) CCTVViewPanel.SetActive(false);
        foreach (var slot in cctvList)
            if (slot.cctvCamera) slot.cctvCamera.enabled = false;
        isViewing = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        var cs = GetCurrentSlot();
        mapPanelManager?.OnEnterMapPanel(cs?.renderTexture, cs?.cameraName, cs?.durability ?? 0, cs?.destructible ?? true, cs?.cameraState ?? CameraState.Active);
    }
    public void HideMinimapPanel()
    {
        if (MinimapPanel) MinimapPanel.SetActive(false);
        if (CCTVViewPanel) CCTVViewPanel.SetActive(true);

        int idx = mapPanelManager.GetLastHoveredIdx();
        if (idx >= 0)
            SetActiveCamera(idx);
        else
            SetActiveCamera(currentCameraIndex);

        isViewing = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // 내부 유틸
    private CCTVSlot GetCurrentSlot() =>
        (currentCameraIndex >= 0 && currentCameraIndex < cctvList.Count) ? cctvList[currentCameraIndex] : null;
    private void ResetRotation()
    {
        cctvYaw = Mathf.Clamp(0f, yawLimit.x, yawLimit.y);
        cctvPitch = Mathf.Clamp(0f, pitchLimit.x, pitchLimit.y);
        var pivot = GetCurrentPivot();
        if (pivot) pivot.localRotation = Quaternion.identity;
    }
    private Transform GetCurrentPivot() => GetCurrentSlot()?.GetPivot();

    // FOV(줌)
    private void SetZoom(bool isZoomIn)
    {
        var cam = GetCurrentCamera();
        if (cam == null) return;
        float targetFOV = isZoomIn ? zoomedFOV : defaultFOV;
        if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
        zoomCoroutine = StartCoroutine(LerpFOV(cam, targetFOV, zoomLerpTime));
    }
    private System.Collections.IEnumerator LerpFOV(Camera cam, float target, float duration)
    {
        float start = cam.fieldOfView;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            cam.fieldOfView = Mathf.Lerp(start, target, t);
            yield return null;
        }
        cam.fieldOfView = target;
    }

#if UNITY_EDITOR
    // 디버그 핫키 포함
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (isViewing) EndCCTVView();
            else StartCCTVView();
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (MinimapPanel.activeSelf) HideMinimapPanel();
            else ShowMinimapPanel();
        }
        if (Input.GetKeyDown(KeyCode.B)) Debug_ToggleBreakCurrentCamera();
        if (Input.GetKeyDown(KeyCode.O)) Debug_ToggleCurrentCameraOff();
        if (Input.GetKeyDown(KeyCode.R)) Debug_RepairCurrentCamera();

        if (isViewing && !isCameraLocked && currentCameraIndex >= 0)
        {
            HandleMouseLook();
            if (Input.GetMouseButtonDown(0)) SetZoom(true);
            if (Input.GetMouseButtonUp(0)) SetZoom(false);
        }
    }
    private void Debug_ToggleBreakCurrentCamera()
    {
        var slot = GetCurrentSlot();
        if (slot == null || !slot.destructible) return;

        if (slot.cameraState == CameraState.Broken)
            slot.Repair();
        else
        {
            slot.durability = 0;
            slot.cameraState = CameraState.Broken;
        }
        UpdateCCTVUI();
    }
    private void Debug_ToggleCurrentCameraOff()
    {
        var slot = GetCurrentSlot();
        if (slot == null) return;
        if (slot.cameraState == CameraState.Broken) return;

        slot.cameraState = (slot.cameraState == CameraState.Off)
            ? CameraState.Active
            : CameraState.Off;
        UpdateCCTVUI();
    }
    private void Debug_RepairCurrentCamera()
    {
        var slot = GetCurrentSlot();
        if (slot == null || !slot.destructible) return;
        slot.Repair();
        UpdateCCTVUI();
    }
#endif

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        if (Mathf.Approximately(mouseX, 0f) && Mathf.Approximately(mouseY, 0f)) return;

        cctvYaw += mouseX * rotationSpeed * Time.deltaTime;
        cctvPitch += (invertY ? mouseY : -mouseY) * rotationSpeed * Time.deltaTime;

        cctvYaw = Mathf.Clamp(cctvYaw, yawLimit.x, yawLimit.y);
        cctvPitch = Mathf.Clamp(cctvPitch, pitchLimit.x, pitchLimit.y);

        var pivot = GetCurrentPivot();
        if (pivot) pivot.localRotation = Quaternion.Euler(cctvPitch, cctvYaw, 0f);
    }

    // Getter
    public Camera GetCurrentCamera() => GetCurrentSlot()?.cctvCamera;
    public GameObject GetCurrentRoomRef() => GetCurrentSlot()?.roomRef;
    public string GetCurrentCameraName() => GetCurrentSlot()?.cameraName ?? "";
    public int GetCurrentDurability() => GetCurrentSlot()?.durability ?? 0;
    public List<CCTVSlot> GetCCTVList() => cctvList;
    public bool IsViewing() => isViewing;
}

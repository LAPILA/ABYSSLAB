//==============================================================================
// CCTV 시스템 총괄 매니저
// - RoomBound와 연동하여 각 방 상태 확인 및 UI 반영
// - 전환 시 전환 이펙트 지원
// - 키보드 또는 UI 버튼을 통한 카메라 제어
//==============================================================================

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using System;

public class CCTVManager : MonoBehaviour
{
    #region 🔧 필드 구성 및 상태

    public static CCTVManager Instance { get; private set; }

    [BoxGroup("CCTV Configs")]
    [SerializeField] private List<CCTVCameraUnit> cameraUnits = new();
    [BoxGroup("CCTV Configs")]
    [SerializeField] private RenderTexture defaultRenderTexture;

    [BoxGroup("Room Reference")]
    [SerializeField] private Transform roomParentRoot;

    [BoxGroup("UI References")]
    [SerializeField] private GameObject cctvWorldCanvas;
    [SerializeField] private RawImage cctvScreen;
    [SerializeField] private Text cameraNameText;
    [SerializeField] private Image alertIcon;
    [SerializeField] private Image destroyedIcon;

    [BoxGroup("Camera Control")]
    [SerializeField] private CinemachineCamera virtualCam;
    [SerializeField] private Transform mainRoomViewpoint;

    [BoxGroup("Input Key Config")]
    [SerializeField] private KeyCode interactKey = KeyCode.T; 
    [SerializeField] private KeyCode nextKey = KeyCode.Q;
    [SerializeField] private KeyCode prevKey = KeyCode.E;

    [BoxGroup("Runtime")]
    [SerializeField] private FirstPersonController playerController;
    [SerializeField] private CinemachineCamera playerCam;

    [BoxGroup("Transition Effect")]
    [SerializeField] private GameObject transitionEffectObject;

    private int currentIndex = -1;
    private bool isViewing = false;

    #endregion

    #region ▶ Unity Loop

    private void Start()
    {
        Instance = this;
        InitializeRooms();
        cctvWorldCanvas?.SetActive(false);
        transitionEffectObject?.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            if (!isViewing) StartCCTVView(0);
            else EndCCTVView();
        }

        if (!isViewing) return;

        if (Input.GetKeyDown(nextKey)) SwitchToCamera((currentIndex + 1) % cameraUnits.Count);
        else if (Input.GetKeyDown(prevKey)) SwitchToCamera((currentIndex - 1 + cameraUnits.Count) % cameraUnits.Count);
    }

    #endregion

    #region ▶ 초기화

    private void InitializeRooms()
    {
        if (roomParentRoot == null) return;

        RoomBound[] roomBounds = roomParentRoot.GetComponentsInChildren<RoomBound>();
        cameraUnits.Clear();

        for (int i = 0; i < roomBounds.Length; i++)
        {
            var rb = roomBounds[i];
            var cam = rb.GetComponentInChildren<Camera>();
            if (cam == null) continue;

            var renderTex = new RenderTexture(1920, 1080, 16);
            cam.targetTexture = renderTex;

            var unit = new CCTVCameraUnit()
            {
                RoomID = i.ToString(),
                Cam = cam,
                OutputTexture = renderTex,
                ViewPoint = cam.transform,
                TargetRoom = rb
            };

            rb.OnDangerLevelChanged += () => { if (rb == GetCurrentRoomBound()) UpdateRoomStatusUI(rb); };
            rb.OnRoomDestroyed += () => { if (rb == GetCurrentRoomBound()) UpdateRoomStatusUI(rb); };

            cameraUnits.Add(unit);
        }
    }

    #endregion

    #region ▶ CCTV 제어 함수

    public void StartCCTVView(int index)
    {
        isViewing = true;
        playerCam.gameObject.SetActive(false);
        virtualCam.gameObject.SetActive(true);

        PlayTransitionEffect(() =>
        {
            SwitchToCamera(index);
            cctvWorldCanvas?.SetActive(true);

            playerController?.StopMove();
            EventSystem.current.sendNavigationEvents = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        });
    }

    public void EndCCTVView()
    {
        isViewing = false;

        PlayTransitionEffect(() =>
        {
            virtualCam.gameObject.SetActive(false);
            playerCam.gameObject.SetActive(true);
            virtualCam.Follow = mainRoomViewpoint;
            virtualCam.LookAt = mainRoomViewpoint;

            cctvScreen.texture = defaultRenderTexture;
            cctvWorldCanvas?.SetActive(false);

            playerController?.StartMove();
            EventSystem.current.sendNavigationEvents = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        });
    }

    public void SwitchToCamera(int index)
    {
        try
        {
            if (cameraUnits == null || cameraUnits.Count == 0)
            {
                Debug.LogWarning("카메라 유닛 없음!");
                return;
            }
            if (index < 0 || index >= cameraUnits.Count)
            {
                Debug.LogWarning("잘못된 카메라 인덱스");
                return;
            }

            currentIndex = index;
            var unit = cameraUnits[index];
            if (unit == null || unit.ViewPoint == null)
            {
                Debug.LogWarning("카메라 유닛 또는 뷰포인트가 null!");
                return;
            }
            var room = unit.TargetRoom;
            bool isCamDisabled = !room.CCTVEnabled;

            PlayTransitionEffect(() =>
            {
                if (virtualCam != null && unit.ViewPoint != null)
                {
                    virtualCam.Follow = unit.ViewPoint;
                    virtualCam.LookAt = unit.ViewPoint;
                }
                if (cctvScreen != null)
                    cctvScreen.texture = (isCamDisabled ? defaultRenderTexture : unit.OutputTexture);
                if (cameraNameText != null)
                    cameraNameText.text = $"CAMERA - {unit.RoomID}";
                if (room != null)
                    UpdateRoomStatusUI(room);
                if (transitionEffectObject != null)
                    transitionEffectObject.SetActive(isCamDisabled);
            });
        }
        catch (Exception e)
        {
            Debug.LogError("CCTV 콜백 예외: " + e);
        }
    }

    private void UpdateRoomStatusUI(RoomBound room)
    {
        if (room == null) return;
        alertIcon.enabled = room.CurrentDangerLevel >= 0.8f;
        destroyedIcon.enabled = room.IsDestroyed;
    }

    public void SwitchToCameraByIndex(int index) => SwitchToCamera(index);
    public CCTVCameraUnit GetCameraUnitByIndex(int index) => index >= 0 && index < cameraUnits.Count ? cameraUnits[index] : null;
    public CCTVCameraUnit GetCurrentUnit() => GetCameraUnitByIndex(currentIndex);
    public RoomBound GetCurrentRoomBound() => GetCurrentUnit()?.TargetRoom;
    public Camera GetCurrentCamera() => GetCurrentUnit()?.Cam;

    private void PlayTransitionEffect(System.Action onComplete)
    {
        if (transitionEffectObject == null)
        {
            onComplete?.Invoke();
            return;
        }

        transitionEffectObject.SetActive(true);
        DOVirtual.DelayedCall(0.2f, () =>
        {
            transitionEffectObject.SetActive(false);
            onComplete?.Invoke();
        });
    }

    #endregion

    #region ▶ 외부 인터페이스

    public void ToggleRoomDoor()
    {
        GetCurrentRoomBound()?.ToggleDoor();
    }

    public void ToggleCameraState()
    {
        var unit = GetCurrentUnit();
        if (unit == null || unit.Cam == null) return;

        unit.Cam.enabled = !unit.Cam.enabled;
        unit.TargetRoom?.UpdateCameraState(unit.Cam.enabled);
    }

    public void HandleCameraStateTransition(bool isEnabled)
    {
        if (transitionEffectObject == null || cctvScreen == null) return;

        if (!isEnabled)
        {
            transitionEffectObject.SetActive(true);
            cctvScreen.texture = defaultRenderTexture;
            UpdateRoomStatusUI(GetCurrentRoomBound());
        }
        else
        {
            transitionEffectObject.SetActive(true);
            DOVirtual.DelayedCall(0.3f, () =>
            {
                var unit = GetCurrentUnit();
                if (unit != null)
                {
                    cctvScreen.texture = unit.OutputTexture;
                }
                transitionEffectObject.SetActive(false);
                UpdateRoomStatusUI(GetCurrentRoomBound());
            });
        }
    }

    #endregion
}

//==============================================================================
// CCTV 유닛 클래스 (Room과 연결된 카메라 단위)
//==============================================================================
[System.Serializable]
public class CCTVCameraUnit
{
    public string RoomID;
    public Camera Cam;
    public RenderTexture OutputTexture;
    public Transform ViewPoint;
    public RoomBound TargetRoom;
}

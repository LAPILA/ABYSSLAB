using System;
using UnityEngine;
using Sirenix.OdinInspector;

public class RoomBound : SerializedMonoBehaviour
{
    [Header("Room 기본 정보")]
    public string RoomID = "Room01";

    [Header("상태 수치")]
    [Range(0f, 1f)] public float CurrentDangerLevel = 0f; // 위험도 (0 = 안전, 1 = 매우 위험)
    public bool IsDestroyed = false;
    public bool IsDoorOpen = false;

    [Header("상호작용 가능 여부")]
    public bool DoorCanBeControlled = true;

    [Header("기타 컴포넌트")]
    public GameObject BoundMonster; // 연동된 괴물 (없을 수도 있음)

    [Header("CCTV 상태")]
    [ReadOnly]
    [ShowInInspector]
    public bool CCTVEnabled = true;

    // 이벤트: CCTVManager 또는 다른 UI 시스템에서 상태 변화 감지용
    public event Action OnDangerLevelChanged = delegate { };
    public event Action OnDoorStateChanged = delegate { };
    public event Action OnRoomDestroyed = delegate { };
    public event Action<bool> OnCCTVStateChanged = delegate { }; // ON/OFF 시각화용 이벤트

    private void Awake()
    {
        // 괴물이 없으면 CCTV 비활성화
        if (BoundMonster == null)
        {
            CCTVEnabled = false;
            Debug.Log($"[RoomBound] {RoomID}: 괴물이 없으므로 CCTV 비활성화됨.");
        }
        else
        {
            CCTVEnabled = true;
        }
    }

    // 위험도 업데이트
    public void SetDangerLevel(float newValue)
    {
        float clamped = Mathf.Clamp01(newValue);
        if (!Mathf.Approximately(CurrentDangerLevel, clamped))
        {
            CurrentDangerLevel = clamped;
            OnDangerLevelChanged?.Invoke();
        }
    }

    // 문 여닫기 토글
    public void ToggleDoor()
    {
        if (!DoorCanBeControlled || IsDestroyed) return;

        IsDoorOpen = !IsDoorOpen;
        OnDoorStateChanged?.Invoke();
    }

    public void OpenDoor()
    {
        if (!IsDoorOpen && DoorCanBeControlled && !IsDestroyed)
        {
            IsDoorOpen = true;
            OnDoorStateChanged?.Invoke();
        }
    }

    public void CloseDoor()
    {
        if (IsDoorOpen && DoorCanBeControlled && !IsDestroyed)
        {
            IsDoorOpen = false;
            OnDoorStateChanged?.Invoke();
        }
    }

    public void MarkAsDestroyed()
    {
        if (!IsDestroyed)
        {
            IsDestroyed = true;
            OnRoomDestroyed?.Invoke();
        }
    }
    public void UpdateCameraState(bool isEnabled)
    {
        CCTVEnabled = isEnabled;
        Debug.Log($"[RoomBound] {RoomID} CCTV 상태: {(isEnabled ? "켜짐" : "꺼짐")}");

        if (!CCTVEnabled)
        {
            MarkAsDestroyed();
        }
        else
        {
            RestoreFromDestroyed();
        }

        OnCCTVStateChanged?.Invoke(isEnabled);

        // Transition 효과도 여기서 담당 (현재 룸일 때만)
        if (CCTVManager.Instance != null && CCTVManager.Instance.GetCurrentRoomBound() == this)
        {
            CCTVManager.Instance.HandleCameraStateTransition(isEnabled);
        }
    }

    public void RestoreFromDestroyed()
    {
        if (IsDestroyed)
        {
            IsDestroyed = false;
            OnRoomDestroyed?.Invoke();
        }
    }

#if UNITY_EDITOR
    private bool lastDestroyedState;

    private void OnValidate()
    {
        if (!Application.isPlaying) return;

        if (lastDestroyedState != IsDestroyed)
        {
            lastDestroyedState = IsDestroyed;

            if (IsDestroyed)
            {
                OnRoomDestroyed?.Invoke();

                if (CCTVManager.Instance != null && CCTVManager.Instance.GetCurrentRoomBound() == this)
                {
                    CCTVManager.Instance.HandleCameraStateTransition(false);
                }
            }
            else
            {
                if (CCTVManager.Instance != null && CCTVManager.Instance.GetCurrentRoomBound() == this)
                {
                    CCTVManager.Instance.HandleCameraStateTransition(true);
                }
            }
        }
    }
#endif

}

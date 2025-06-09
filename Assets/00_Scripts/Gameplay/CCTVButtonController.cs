using UnityEngine;

public class CCTVButtonController : MonoBehaviour
{
    [SerializeField] private CCTVManager cctvManager;

    // ===== 카메라 전환 버튼들 =====
    public void SwitchToCamera_Room1() => cctvManager.SwitchToCameraByIndex(0);
    public void SwitchToCamera_Room2() => cctvManager.SwitchToCameraByIndex(1);
    public void SwitchToCamera_Room3() => cctvManager.SwitchToCameraByIndex(2);
    public void SwitchToCamera_Room4() => cctvManager.SwitchToCameraByIndex(3);

    // ===== 문 열기/닫기 토글 =====
    public void ToggleDoor() => cctvManager.ToggleRoomDoor();

    // ===== 카메라 ON/OFF 토글 =====
    public void ToggleCamera() => cctvManager.ToggleCameraState();
}

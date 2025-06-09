using UnityEngine;

public class CCTVButtonController : MonoBehaviour
{
    [SerializeField] private CCTVManager cctvManager;

    // ===== ī�޶� ��ȯ ��ư�� =====
    public void SwitchToCamera_Room1() => cctvManager.SwitchToCameraByIndex(0);
    public void SwitchToCamera_Room2() => cctvManager.SwitchToCameraByIndex(1);
    public void SwitchToCamera_Room3() => cctvManager.SwitchToCameraByIndex(2);
    public void SwitchToCamera_Room4() => cctvManager.SwitchToCameraByIndex(3);

    // ===== �� ����/�ݱ� ��� =====
    public void ToggleDoor() => cctvManager.ToggleRoomDoor();

    // ===== ī�޶� ON/OFF ��� =====
    public void ToggleCamera() => cctvManager.ToggleCameraState();
}

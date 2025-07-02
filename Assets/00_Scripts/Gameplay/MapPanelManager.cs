//==============================================================================
// MapPanelManager (���̶�Ű ���� ��ġ, Preview/Info/��ư/������ ���±��� last�� ����)
//==============================================================================

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine.EventSystems;

public class MapPanelManager : MonoBehaviour
{
    [Title("UI ���۷���")]
    [SerializeField, Required] private RawImage mapBG;
    [SerializeField, Required] private List<Button> roomButtons;
    [SerializeField, Required] private RawImage previewRawImage;
    [SerializeField, Required] private Text nameText;
    [SerializeField, Required] private Text durabilityText;
    [SerializeField, Required] private GameObject brokeIcon;
    [SerializeField, Required] private GameObject offIcon;

    [Title("CCTV ������")]
    [SerializeField, Required] private CCTVManager cctvManager;

    [Title("��ư �� CCTVSlot �ε��� ����")]
    [SerializeField, Required] private List<int> cctvSlotIndices;

    // ���������� Hover�� �ε���/�̸����� ���� ���� (Map ���� �� ����)
    private int lastHoveredIdx = -1;
    private RenderTexture lastPreviewTex = null;
    private string lastName = "";
    private string lastDurability = "";
    private bool lastBrokeIcon = false;
    private bool lastOffIcon = false;

    private void Awake()
    {
        if (roomButtons.Count != cctvSlotIndices.Count)
        {
            Debug.LogError("[MapPanelManager] roomButtons�� cctvSlotIndices ���� ����ġ!");
            enabled = false;
            return;
        }

        for (int i = 0; i < roomButtons.Count; ++i)
        {
            int slotIdx = cctvSlotIndices[i];
            var btn = roomButtons[i];
            EventTrigger trigger = btn.GetComponent<EventTrigger>();
            if (!trigger) trigger = btn.gameObject.AddComponent<EventTrigger>();
            AddEvent(trigger, EventTriggerType.PointerEnter, () => OnRoomButtonHover(slotIdx));
            AddEvent(trigger, EventTriggerType.PointerExit, () => OnRoomButtonExit(slotIdx));
            btn.onClick.AddListener(() => OnRoomButtonClick(slotIdx));
        }
        ResetInfoPanel();
    }

    private void AddEvent(EventTrigger trigger, EventTriggerType type, System.Action action)
    {
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(_ => action());
        trigger.triggers.Add(entry);
    }

    /// <summary>
    /// InfoPanel UI ���� �� �̸����� ���� ���� (Map ���Խ� �׻� ȣ��)
    /// </summary>
    public void ResetInfoPanel()
    {
        previewRawImage.texture = lastPreviewTex;
        nameText.text = lastName;
        durabilityText.text = lastDurability;
        brokeIcon.SetActive(lastBrokeIcon);
        offIcon.SetActive(lastOffIcon);
    }

    /// <summary>
    /// �� ���� �� ������ �̸�����/�ؽ�Ʈ ���� ����
    /// </summary>
    public void OnEnterMapPanel(RenderTexture tex, string camName, int durability, bool destructible, CameraState state)
    {
        lastPreviewTex = tex;
        lastName = camName ?? "";
        lastDurability = destructible ? $"������: {durability}" : "������: ��";
        lastBrokeIcon = state == CameraState.Broken;
        lastOffIcon = (state == CameraState.Off || state == CameraState.Broken);
        ResetInfoPanel();
    }

    /// <summary>
    /// RoomButton Hover �� Info/Preview/������ ���� + ������ Hover �ε��� ����
    /// </summary>
    public void OnRoomButtonHover(int idx)
    {
        var slots = cctvManager.GetCCTVList();
        if (idx < 0 || idx >= slots.Count) return;
        var slot = slots[idx];

        lastHoveredIdx = idx;
        previewRawImage.texture = slot.renderTexture;
        nameText.text = slot.cameraName;
        durabilityText.text = slot.destructible ? $"������: {slot.durability}" : "������: ��";

        bool isBroken = slot.cameraState == CameraState.Broken;
        bool isOff = slot.cameraState == CameraState.Off || slot.cameraState == CameraState.Broken;

        brokeIcon.SetActive(isBroken);
        offIcon.SetActive(isOff);

        // �̸�����/�ؽ�Ʈ/������ ���� ��� (Map �г� ����/���ͽ� �ݿ�)
        lastPreviewTex = slot.renderTexture;
        lastName = slot.cameraName;
        lastDurability = slot.destructible ? $"������: {slot.durability}" : "������: ��";
        lastBrokeIcon = isBroken;
        lastOffIcon = isOff;
    }

    /// <summary>
    /// RoomButton Exit �� InfoPanel ����(������ ���� ����)
    /// </summary>
    public void OnRoomButtonExit(int idx)
    {
        ResetInfoPanel();
    }

    /// <summary>
    /// RoomButton Ŭ�� �� �ش� CCTV�� ��ȯ, �� �г� �ݱ�
    /// </summary>
    public void OnRoomButtonClick(int idx)
    {
        cctvManager.SwitchToCameraByIndex(idx);
        cctvManager.HideMinimapPanel();
        cctvManager.StartCCTVView();
    }

    /// <summary>
    /// ���������� Hover�� CCTV �ε��� (���ͽ� ���)
    /// </summary>
    public int GetLastHoveredIdx() => lastHoveredIdx;
}

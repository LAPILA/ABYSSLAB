//==============================================================================
// MapPanelManager (하이라키 직접 배치, Preview/Info/버튼/아이콘 상태까지 last로 유지)
//==============================================================================

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine.EventSystems;

public class MapPanelManager : MonoBehaviour
{
    [Title("UI 레퍼런스")]
    [SerializeField, Required] private RawImage mapBG;
    [SerializeField, Required] private List<Button> roomButtons;
    [SerializeField, Required] private RawImage previewRawImage;
    [SerializeField, Required] private Text nameText;
    [SerializeField, Required] private Text durabilityText;
    [SerializeField, Required] private GameObject brokeIcon;
    [SerializeField, Required] private GameObject offIcon;

    [Title("CCTV 데이터")]
    [SerializeField, Required] private CCTVManager cctvManager;

    [Title("버튼 ↔ CCTVSlot 인덱스 매핑")]
    [SerializeField, Required] private List<int> cctvSlotIndices;

    // 마지막으로 Hover한 인덱스/미리보기 상태 저장 (Map 진입 시 복원)
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
            Debug.LogError("[MapPanelManager] roomButtons와 cctvSlotIndices 개수 불일치!");
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
    /// InfoPanel UI 리셋 및 미리보기 상태 복원 (Map 진입시 항상 호출)
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
    /// 맵 진입 시 마지막 미리보기/텍스트 상태 복원
    /// </summary>
    public void OnEnterMapPanel(RenderTexture tex, string camName, int durability, bool destructible, CameraState state)
    {
        lastPreviewTex = tex;
        lastName = camName ?? "";
        lastDurability = destructible ? $"내구도: {durability}" : "내구도: ∞";
        lastBrokeIcon = state == CameraState.Broken;
        lastOffIcon = (state == CameraState.Off || state == CameraState.Broken);
        ResetInfoPanel();
    }

    /// <summary>
    /// RoomButton Hover → Info/Preview/아이콘 갱신 + 마지막 Hover 인덱스 저장
    /// </summary>
    public void OnRoomButtonHover(int idx)
    {
        var slots = cctvManager.GetCCTVList();
        if (idx < 0 || idx >= slots.Count) return;
        var slot = slots[idx];

        lastHoveredIdx = idx;
        previewRawImage.texture = slot.renderTexture;
        nameText.text = slot.cameraName;
        durabilityText.text = slot.destructible ? $"내구도: {slot.durability}" : "내구도: ∞";

        bool isBroken = slot.cameraState == CameraState.Broken;
        bool isOff = slot.cameraState == CameraState.Off || slot.cameraState == CameraState.Broken;

        brokeIcon.SetActive(isBroken);
        offIcon.SetActive(isOff);

        // 미리보기/텍스트/아이콘 상태 기록 (Map 패널 종료/복귀시 반영)
        lastPreviewTex = slot.renderTexture;
        lastName = slot.cameraName;
        lastDurability = slot.destructible ? $"내구도: {slot.durability}" : "내구도: ∞";
        lastBrokeIcon = isBroken;
        lastOffIcon = isOff;
    }

    /// <summary>
    /// RoomButton Exit → InfoPanel 리셋(마지막 상태 복원)
    /// </summary>
    public void OnRoomButtonExit(int idx)
    {
        ResetInfoPanel();
    }

    /// <summary>
    /// RoomButton 클릭 → 해당 CCTV로 전환, 맵 패널 닫기
    /// </summary>
    public void OnRoomButtonClick(int idx)
    {
        cctvManager.SwitchToCameraByIndex(idx);
        cctvManager.HideMinimapPanel();
        cctvManager.StartCCTVView();
    }

    /// <summary>
    /// 마지막으로 Hover된 CCTV 인덱스 (복귀시 사용)
    /// </summary>
    public int GetLastHoveredIdx() => lastHoveredIdx;
}

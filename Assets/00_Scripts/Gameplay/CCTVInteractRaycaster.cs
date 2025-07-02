using UnityEngine;
using UnityEngine.UI;

/// CCTV 화면 위에
///   ─ 노란 박스  ─ 오브젝트 이름(Text)
/// 만 표시한다 (Leader Line 제거 버전)
public class CCTVInteractRaycaster_UI : MonoBehaviour
{
    /* --------- 인스펙터 --------- */
    [Header("참조")]
    [SerializeField] CCTVManager cctv;
    [SerializeField] RectTransform rawImage;   // CCTV RawImage
    [SerializeField] RectTransform box;        // 노란 박스 (Outline Image)
    [SerializeField] RectTransform label;      // 오브젝트 이름(Text)

    [Header("Ray 설정")]
    [SerializeField] float rayDistance = 20f;

    [Header("UI 옵션")]
    [SerializeField, Range(0, .3f)] float paddingPct = .10f;
    [SerializeField] float boxFollowLerp = 12f;
    [SerializeField] float labelFollowLerp = 6f;
    [SerializeField] float gapBox2Label = 6f;

    /* --------- 내부 --------- */
    BaseInteractable curTarget;
    Collider curCol;
    static readonly Vector3[] corners = new Vector3[8];

    /* =========================================================== */
    void Update()
    {
        if (cctv == null || !cctv.IsViewing()) { ClearUI(); return; }

        Camera cam = cctv.GetCurrentCamera();
        if (!cam) { ClearUI(); return; }

        Ray ray = cam.ViewportPointToRay(new Vector3(.5f, .5f));
        if (Physics.Raycast(ray, out var hit, rayDistance) &&
            hit.collider.CompareTag("InteractObj"))
        {
            if (curCol != hit.collider)            // 새 대상일 때만 OnHover 갱신
            {
                ClearUI();
                curCol = hit.collider;
                curTarget = curCol.GetComponent<BaseInteractable>();
                curTarget?.OnHover();
            }

            if (curTarget && Input.GetKeyDown(KeyCode.E))
                curTarget.Interact();

            UpdateUI(cam, curCol.bounds);
        }
        else ClearUI();
    }

    /* ============= UI 갱신 핵심 ============= */
    void UpdateUI(Camera cam, Bounds b)
    {
        /* 1) Bounds 8점 → Viewport AABB */
        Vector3 c = b.center, e = b.extents;
        corners[0] = c + new Vector3(-e.x, -e.y, -e.z);
        corners[1] = c + new Vector3(-e.x, -e.y, e.z);
        corners[2] = c + new Vector3(-e.x, e.y, -e.z);
        corners[3] = c + new Vector3(-e.x, e.y, e.z);
        corners[4] = c + new Vector3(e.x, -e.y, -e.z);
        corners[5] = c + new Vector3(e.x, -e.y, e.z);
        corners[6] = c + new Vector3(e.x, e.y, -e.z);
        corners[7] = c + new Vector3(e.x, e.y, e.z);

        float minX = 1, minY = 1, maxX = 0, maxY = 0;
        for (int i = 0; i < 8; ++i)
        {
            Vector3 vp = cam.WorldToViewportPoint(corners[i]);
            if (vp.z < 0) return;            // 일부라도 카메라 뒤면 표시 안 함
            minX = vp.x < minX ? vp.x : minX;
            minY = vp.y < minY ? vp.y : minY;
            maxX = vp.x > maxX ? vp.x : maxX;
            maxY = vp.y > maxY ? vp.y : maxY;
        }

        /* 2) 패딩 */
        float padX = (maxX - minX) * paddingPct;
        float padY = (maxY - minY) * paddingPct;
        minX -= padX; maxX += padX;
        minY -= padY; maxY += padY;

        /* 3) Viewport → RawImage Local 좌표 */
        Rect raw = rawImage.rect;
        float rw = raw.width, rh = raw.height;
        Vector2 minL = new((minX - .5f) * rw, (minY - .5f) * rh);
        Vector2 maxL = new((maxX - .5f) * rw, (maxY - .5f) * rh);

        Vector2 boxPos = (minL + maxL) * .5f;
        Vector2 boxSize = new(Mathf.Abs(maxL.x - minL.x),
                              Mathf.Abs(maxL.y - minL.y));

        /* 4) 박스 부드럽게 이동·크기 */
        box.anchoredPosition = Vector2.Lerp(box.anchoredPosition, boxPos, Time.deltaTime * boxFollowLerp);
        Vector2 curS = Vector2.Lerp(box.sizeDelta, boxSize, Time.deltaTime * boxFollowLerp);
        box.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, curS.x);
        box.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, curS.y);
        if (!box.gameObject.activeSelf) box.gameObject.SetActive(true);

        /* -------- 라벨 배치 -------- */
        bool placeRight = (rawImage.anchoredPosition.x + boxPos.x) < 0;  // Canvas 중심 기준
        float halfW = curS.x * 0.5f;

        float targetLblX = boxPos.x + (placeRight ? (halfW + gapBox2Label)
                                                  : -(halfW + gapBox2Label));
        Vector2 targetLblPos = new(targetLblX, boxPos.y);

        label.pivot = placeRight ? new Vector2(0, .5f) : new Vector2(1, .5f);
        label.anchoredPosition = Vector2.Lerp(label.anchoredPosition, targetLblPos, Time.deltaTime * labelFollowLerp);
        label.GetComponent<Text>().text = curTarget ? curTarget.name : "";
        if (!label.gameObject.activeSelf) label.gameObject.SetActive(true);
    }

    /* -------- 클리어 & 해제 -------- */
    void ClearUI()
    {
        curTarget?.OnUnhover();
        curTarget = null; curCol = null;

        if (box.gameObject.activeSelf) box.gameObject.SetActive(false);
        if (label.gameObject.activeSelf) label.gameObject.SetActive(false);
    }
}

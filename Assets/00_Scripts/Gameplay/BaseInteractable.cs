using UnityEngine;

/// <summary>
/// 모든 상호작용 오브젝트의 베이스 추상 클래스.
/// 반드시 Interact() 구현 필요.
/// </summary>
public abstract class BaseInteractable : MonoBehaviour
{
    [SerializeField] private Renderer highlightRenderer;
    [SerializeField] private Color highlightColor = Color.yellow;
    private Color? originalColor = null;

    /// <summary>
    /// 플레이어/CCTV에 의해 조준 시작시 호출 (하이라이트 ON)
    /// </summary>
    public virtual void OnHover()
    {
        if (highlightRenderer != null)
        {
            if (originalColor == null)
                originalColor = highlightRenderer.material.color;
            highlightRenderer.material.color = highlightColor;
        }
    }

    /// <summary>
    /// 조준 해제시 호출 (하이라이트 OFF)
    /// </summary>
    public virtual void OnUnhover()
    {
        if (highlightRenderer != null && originalColor != null)
            highlightRenderer.material.color = originalColor.Value;
    }

    /// <summary>
    /// 실제 인터랙트(E키 등)시 호출 (자식에서 반드시 구현)
    /// </summary>
    public abstract void Interact();
}

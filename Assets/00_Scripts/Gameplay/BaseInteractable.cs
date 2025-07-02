using UnityEngine;

/// <summary>
/// ��� ��ȣ�ۿ� ������Ʈ�� ���̽� �߻� Ŭ����.
/// �ݵ�� Interact() ���� �ʿ�.
/// </summary>
public abstract class BaseInteractable : MonoBehaviour
{
    [SerializeField] private Renderer highlightRenderer;
    [SerializeField] private Color highlightColor = Color.yellow;
    private Color? originalColor = null;

    /// <summary>
    /// �÷��̾�/CCTV�� ���� ���� ���۽� ȣ�� (���̶���Ʈ ON)
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
    /// ���� ������ ȣ�� (���̶���Ʈ OFF)
    /// </summary>
    public virtual void OnUnhover()
    {
        if (highlightRenderer != null && originalColor != null)
            highlightRenderer.material.color = originalColor.Value;
    }

    /// <summary>
    /// ���� ���ͷ�Ʈ(EŰ ��)�� ȣ�� (�ڽĿ��� �ݵ�� ����)
    /// </summary>
    public abstract void Interact();
}

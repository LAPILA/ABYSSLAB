using UnityEngine;

/// <summary>
/// ����׿� ��ȣ�ۿ� ������Ʈ ���� ����.
/// </summary>
public class DebugInteractable : BaseInteractable
{
    public override void Interact()
    {
        Debug.Log($"[Interactable] {name}��(��) ��ȣ�ۿ��!");
    }
}

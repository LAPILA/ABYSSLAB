using UnityEngine;

/// <summary>
/// 디버그용 상호작용 오브젝트 구현 예시.
/// </summary>
public class DebugInteractable : BaseInteractable
{
    public override void Interact()
    {
        Debug.Log($"[Interactable] {name}이(가) 상호작용됨!");
    }
}

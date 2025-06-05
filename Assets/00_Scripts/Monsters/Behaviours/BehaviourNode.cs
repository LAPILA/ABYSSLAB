using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BehaviourNode : MonoBehaviour
{
    public enum BehaviourNodeStatus
    {
        Ready,
        Running,
        Failed,
        Success
    }

    [SerializeField]
    public BehaviourNodeStatus status;
    private IEnumerator coroutine = null;

    public abstract IEnumerator Visit();
    public void StartBehaviour()
    {

        coroutine = Visit();
        StartCoroutine(coroutine);
    }

    public void StopBehaviour()
    {
        if (coroutine == null)
            return;
        StopCoroutine(coroutine);
    }
}

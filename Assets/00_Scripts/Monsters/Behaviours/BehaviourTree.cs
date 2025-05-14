using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourTree : MonoBehaviour
{
    [SerializeField]
    public List<BehaviourNode> nodes;

    [SerializeField]
    int node_idx = 0;

    void Start()
    {
        if (nodes.Count > 0)
            nodes[node_idx].StartBehaviour();
    }

    void Update()
    {
        BehaviourNode node = nodes[node_idx];
        if (node.status == BehaviourNode.BehaviourNodeStatus.Success)
        {
            node.StopBehaviour();
            node_idx = (node_idx + 1) % nodes.Count;
            nodes[node_idx].StartBehaviour();
        }
    }
}

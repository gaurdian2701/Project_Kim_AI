using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequenceNode : Node
{
    
    public SequenceNode(List<Node> children, Dictionary<string, object> blackboard) : base(children, blackboard)
    {
    }

    public override NodeStates Evaluate()
    {
        if (currentState == NodeStates.RUNNING)
            return currentState;
        
        foreach (Node child in children)
        {
            switch (child.Evaluate())
            {
                case NodeStates.SUCCESS:
                    currentState = NodeStates.SUCCESS;
                    continue;
                case NodeStates.RUNNING:
                    currentState = NodeStates.RUNNING;
                    return currentState;
                default:
                case NodeStates.FAILURE:
                    currentState = NodeStates.FAILURE;
                    return currentState;
            }
        }
        return currentState;
    }
}

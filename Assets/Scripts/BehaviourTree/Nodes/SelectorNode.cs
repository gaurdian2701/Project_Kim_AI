using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectorNode : Node
{
    public SelectorNode(List<Node> children, Dictionary<string, object> blackboard) : base(children, blackboard)
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
                    return currentState;
                case NodeStates.RUNNING:
                    currentState = NodeStates.RUNNING;
                    return currentState;
                case NodeStates.FAILURE:
                    currentState = NodeStates.FAILURE;
                    continue;
            }
        }

        return currentState;
    }

}

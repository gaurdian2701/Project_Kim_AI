using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootNode : Node
{
    public RootNode(List<Node> children, Dictionary<string, object> blackboard) : base(children, blackboard)
    {
    }

    public override NodeStates Evaluate()
    {
        return children[0].Evaluate();
    }
}

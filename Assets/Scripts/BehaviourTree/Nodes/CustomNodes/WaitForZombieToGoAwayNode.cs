using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitForZombieToGoAwayNode : Node
{
    private Kim kimScript;
    
    public WaitForZombieToGoAwayNode(List<Node> children, Dictionary<string, object> blackboard) : base(children,
        blackboard)
    {
        kimScript = blackboard["KimScript"] as Kim;
    }

    public override NodeStates Evaluate()
    {
        Debug.Log("WAITING FOR ZOMBIE TO GO AWAY");
        kimScript.ClearWalkBuffer();
        myBlackboard["PlayerIsWaitingForOpening"] = true;
        return NodeStates.SUCCESS;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootNode : Node
{
    private Kim kimScript;
    public RootNode(Node child, Dictionary<string, object> blackboard) : base(child, blackboard)
    {
        kimScript = blackboard["KimScript"] as Kim;
    }

    public override NodeStates Evaluate()
    {
        if (kimScript.BurgerCollected)
        {
            myBlackboard["CalculatePath"] = true;
            myBlackboard["CurrentTargetTile"] = kimScript.GetNextTarget();
            kimScript.BurgerCollected = false;
        }
        
        return children[0].Evaluate();
    }
}

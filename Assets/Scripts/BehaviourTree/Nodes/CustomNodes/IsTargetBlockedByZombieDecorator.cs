using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class IsTargetBlockedByZombieDecorator : Node
{
    private Kim kimScript;
    public IsTargetBlockedByZombieDecorator(Node child, Dictionary<string, object> blackboard) : base(child, blackboard)
    {
        kimScript = myBlackboard["KimScript"] as Kim;
    }

    public override NodeStates Evaluate()
    {
        if ((myBlackboard["CurrentTargetTile"] as Grid.Tile).IsPartOfZombie)
        {
            Debug.Log("TARGET BLOCKED BY ZOMBIE");
            return children[0].Evaluate();   
        }
        
        Debug.Log("TARGET NOT BLOCKED BY ZOMBIE"); 
        
        // myBlackboard["CalculatePath"] = true;
        return NodeStates.FAILURE;
    }
}

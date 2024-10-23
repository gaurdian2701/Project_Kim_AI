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
            return children[0].Evaluate();   
        
        return NodeStates.FAILURE;
    }
}

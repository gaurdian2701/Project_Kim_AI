using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsZombieTooCloseDecorator : Node
{
    private Kim kimScript;
    public IsZombieTooCloseDecorator(Node child, Dictionary<string, object> blackboard) : base(child, blackboard)
    {
        kimScript = blackboard["KimScript"] as Kim;
    }

    public override NodeStates Evaluate()
    {
        if (Grid.Instance.GetClosest(kimScript.transform.position).IsPartOfZombie)
        {
            Debug.Log("ZOMBIE TOO CLOSE");
            myBlackboard["CurrentTargetTile"] = Grid.Instance.GetClosest
            (kimScript.transform.position - (myBlackboard["ClosestZombie"] as Zombie).transform.position);
            myBlackboard["CalculatePath"] = true;
            return children[0].Evaluate();
        }
        
        Debug.Log("ZOMBIE NOT CLOSE");
        myBlackboard["CurrentTargetTile"] = kimScript.GetNextTarget();
        return NodeStates.FAILURE;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourTree
{
    protected Dictionary<string, object> blackBoard;
    protected Node rootNode;

    protected virtual void FillBlackboard()
    {
    }

    protected virtual void FillTree()
    {
    }
}

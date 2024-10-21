using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourTree : MonoBehaviour
{
    protected Dictionary<string, object> blackBoard;
    protected Node rootNode;

    protected virtual void FillBlackboard()
    {
    }
}

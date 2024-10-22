using System.Collections.Generic;

public class Node
{
    protected List<Node> children;
    protected NodeStates currentState;
    protected Dictionary<string, object> myBlackboard = new Dictionary<string, object>();

    public Node(List<Node> children, Dictionary<string, object> blackboard)
    {
        this.children = children;
        myBlackboard = blackboard;
    }

    public Node(Node child, Dictionary<string, object> blackboard)
    {
        children = new List<Node>{ child };
        myBlackboard = blackboard;
    }
    
    public virtual NodeStates Evaluate() => NodeStates.FAILURE;
}
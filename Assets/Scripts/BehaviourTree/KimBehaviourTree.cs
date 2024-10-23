using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class KimBehaviourTree : BehaviourTree
{ 
    private Kim kimScript;
    private RootNode rootNode;

    public KimBehaviourTree(Kim kimScript)
    {
        this.kimScript = kimScript;
        Init();
    }
    private void Init()
    {
        FillBlackboard();
        FillTree();
        blackBoard["CurrentTargetTile"] = kimScript.GetNextTarget();
    }
    public void Update()
    {
        Debug.Log(blackBoard["CalculatePath"]);
        rootNode.Evaluate();
    }
    
    protected override void FillBlackboard()
    {
        blackBoard = new Dictionary<string, object>();
        blackBoard.Add("CalculatePath", true);
        blackBoard.Add("ZombieIsMoving", false);
        blackBoard.Add("ContextRadius", kimScript.ContextRadius);
        blackBoard.Add("TilesAroundZombie", new List<Grid.Tile>());
        blackBoard.Add("ClosestZombie", null);
        blackBoard.Add("KimScript", kimScript);
        blackBoard.Add("CurrentTargetTile", new Grid.Tile());
    }

    protected override void FillTree()
    {
        UpdatePathNode updatePathToNextTarget = new UpdatePathNode(null, blackBoard, 1);
        UpdatePathNode updatePathToFlee = new UpdatePathNode(null, blackBoard, 2);
        CheckForZombiesOnPathNode checkForZombiesOnPath = new CheckForZombiesOnPathNode(null, blackBoard);
        
        IsZombieTooCloseDecorator isZombieTooClose = new IsZombieTooCloseDecorator(updatePathToFlee, blackBoard);
        WaitForZombieToGoAwayNode waitForZombieToGoAway = new WaitForZombieToGoAwayNode(null, blackBoard);  
        IsTargetBlockedByZombieDecorator isTargetBlockedByZombie = new IsTargetBlockedByZombieDecorator(waitForZombieToGoAway, blackBoard);
        
        SelectorNode zombieSelectorNode = new SelectorNode( new List<Node>{isZombieTooClose, isTargetBlockedByZombie}, blackBoard);
        
        SequenceNode zombieSequenceNode = new SequenceNode(new List<Node>{checkForZombiesOnPath, zombieSelectorNode}, blackBoard);
        
        SelectorNode mainSelectorNode = new SelectorNode(new List<Node>{updatePathToNextTarget, zombieSequenceNode}, blackBoard);
        
        rootNode = new RootNode(mainSelectorNode, blackBoard);
    }
}

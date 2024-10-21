using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class KimBehaviourTree : BehaviourTree
{
    [FormerlySerializedAs("KimScript")] [SerializeField] private Kim kimScript;
    private Zombie closestZombie;

    private void Awake()
    {
        FillBlackboard();
    }

    private void FillBlackboard()
    {
        blackBoard.Add("CalculatePath", true);
        blackBoard.Add("ContextRadius", kimScript.ContextRadius);
        blackBoard.Add("TilesAroundZombie", new List<Grid.Tile>());
        blackBoard.Add("ClosestZombie", closestZombie);
    }
}

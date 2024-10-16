using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class PathNode
{
    public Grid.Tile CurrentTile;
    [FormerlySerializedAs("ParentTile")] public PathNode ParentNode;
    public float CostToMoveToTile;
    public float HeuristicCost;

    public float TotalCost
    {
        get => CostToMoveToTile + HeuristicCost;
    }
}
using System;
using UnityEngine;

[Serializable]
public class TileCost
{
    public Grid.Tile Tile;
    public float CostToMoveToTile;
    public float HeuristicCost;

    public float TotalCost
    {
        get => CostToMoveToTile + HeuristicCost;
    }
}
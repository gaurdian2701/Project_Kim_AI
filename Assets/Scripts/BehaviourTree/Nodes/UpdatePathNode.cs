using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdatePathNode : Node
{
    private Kim kimScript;
    
    public UpdatePathNode(List<Node> children, Dictionary<string, object> blackboard) : base(children,
        blackboard)
    {
        kimScript = blackboard["KimScript"] as Kim;
    }

    public override NodeStates Evaluate() => FindPathToTarget();

    private NodeStates FindPathToTarget()
    {
        Grid.Tile targetTile = myBlackboard["CurrentTargetTile"] as Grid.Tile;
        List<Grid.Tile> openSet = new List<Grid.Tile>();
        HashSet<Grid.Tile> closedSet = new HashSet<Grid.Tile>();

        Grid.Tile startTile = Grid.Instance.GetClosest(kimScript.gameObject.transform.position);

        startTile.CostToMoveToTile = 0;
        startTile.HeuristicCost = GetDistanceBetweenTiles(startTile, targetTile);
        openSet.Add(startTile);

        while (openSet.Count > 0)
        {
            Grid.Tile currentTile = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].TotalCost < currentTile.TotalCost || openSet[i].TotalCost == currentTile.TotalCost)
                    if (openSet[i].HeuristicCost < currentTile.HeuristicCost)
                        currentTile = openSet[i];
            }

            currentTile.IsOnPlayerPath = true;
            openSet.Remove(currentTile);
            closedSet.Add(currentTile);

            if (Grid.Instance.IsSameTile(currentTile, targetTile))
            {
                kimScript.SetWalkBuffer(GetRetracedPathToTargetTile(startTile, targetTile));
                return NodeStates.SUCCESS;
            }

            List<Grid.Tile> neighbours = GetNeighboursOfTile(currentTile);

            for (int i = 0; i < neighbours.Count; i++)
            {
                if (closedSet.Contains(neighbours[i]) || neighbours[i].Occupied ||
                    (myBlackboard["TilesAroundZombie"] as List<Grid.Tile>).Contains(neighbours[i]))
                    continue;

                int newMovementCostToNode = GetDistanceBetweenTiles(currentTile, neighbours[i]);

                if (newMovementCostToNode < neighbours[i].CostToMoveToTile || !openSet.Contains(neighbours[i]))
                {
                    neighbours[i].CostToMoveToTile = newMovementCostToNode;
                    neighbours[i].HeuristicCost = GetDistanceBetweenTiles(targetTile, neighbours[i]);
                    neighbours[i].ParentTile = currentTile;

                    if (!openSet.Contains(neighbours[i]))
                        openSet.Add(neighbours[i]);
                }
            }
        }

        return NodeStates.FAILURE;
    }
    
    private int GetDistanceBetweenTiles(Grid.Tile tile1, Grid.Tile tile2)
    {
        int distanceX = Mathf.Abs(tile1.x - tile2.x);
        int distanceY = Mathf.Abs(tile1.y - tile2.y);

        if (distanceX > distanceY)
            return 14 * distanceY + 10 * (distanceX - distanceY);

        return 14 * distanceX + 10 * (distanceY - distanceX);
    }
    
    private List<Grid.Tile> GetRetracedPathToTargetTile(Grid.Tile startTile, Grid.Tile currentTile)
    {
        List<Grid.Tile> retracedPath = new List<Grid.Tile>();

        while (currentTile != startTile)
        {
            retracedPath.Add(currentTile);
            currentTile = currentTile.ParentTile;
        }

        retracedPath.Reverse();
        return retracedPath;
    }
    
    
    private List<Grid.Tile> GetNeighboursOfTile(Grid.Tile centerNode)
    {
        List<Grid.Tile> neighbours = new List<Grid.Tile>();
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                    continue;

                Vector2Int tilePosition = new Vector2Int(centerNode.x + i, centerNode.y + j);
                Grid.Tile tileCorrespondingToNeighbour = Grid.Instance.TryGetTile(tilePosition);
                if (tileCorrespondingToNeighbour != null)
                    neighbours.Add(tileCorrespondingToNeighbour);
            }
        }

        return neighbours;
    }
}

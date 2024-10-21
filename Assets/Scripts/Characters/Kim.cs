using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Random = System.Random;

public class Kim : CharacterController
{
    public float ContextRadius;

    private Grid gridManager;
    private bool calculatePath = true;
    private List<Grid.Tile> tilesAroundZombie = new List<Grid.Tile>();
    private Zombie closestZombie;

    public override void StartCharacter()
    {
        base.StartCharacter();
        gridManager = Grid.Instance;
        myCurrentTile = gridManager.GetClosest(transform.position);
    }

    public override void UpdateCharacter()
    {
        base.UpdateCharacter();
        closestZombie = GetClosest(GetContextByTag("Zombie"))?.GetComponent<Zombie>();

        if (closestZombie != null)
        {
            GetTileCostsAroundZombie(closestZombie);

            if (TileNotReachable(GetNextTarget()))
            {
                if (Vector3.Distance(transform.position, closestZombie.transform.position) > ContextRadius)
                {
                    myWalkBuffer.Clear();
                    calculatePath = false;
                    return;
                }

                FindPathToTarget(gridManager.GetClosest(transform.position - closestZombie.transform.position));
                calculatePath = false;
                return;
            } //Do this part in CheckForClosestZombie in behaviour tree

            calculatePath = true;
        }
        else
        {
            ClearZombieData();
            calculatePath = true;
        }

        if (calculatePath)
            FindPathToTarget(GetNextTarget());
    }

    private void GetTileCostsAroundZombie(Zombie closestZombie)
    {
        Grid.Tile centerTile = gridManager.GetClosest(closestZombie.transform.position);
        tilesAroundZombie.Add(centerTile);
        centerTile.IsPartOfZombie = true;
        tilesAroundZombie.AddRange(GetNeighbourTilesOfZombie(centerTile));
        foreach (Grid.Tile t in tilesAroundZombie)
        {
            t.CostToMoveToTile = int.MaxValue;
            t.IsPartOfZombie = true;
        }
    }

    private List<Grid.Tile> GetNeighbourTilesOfZombie(Grid.Tile centerTile)
    {
        ClearZombieData();
        List<Grid.Tile> neighbourTiles = new List<Grid.Tile>();
        for (int i = -3; i < 3; i++)
        {
            for (int j = -3; j < 3; j++)
            {
                if (i == 0 && j == 0)
                    continue;

                Grid.Tile tileReturned = gridManager.TryGetTile(new Vector2Int(centerTile.x + i, centerTile.y + j));
                if (tileReturned != null)
                    neighbourTiles.Add(tileReturned);
            }
        }

        return neighbourTiles;
    }


    private void ClearZombieData()
    {
        foreach (Grid.Tile t in tilesAroundZombie)
        {
            if (t == null)
                continue;

            t.CostToMoveToTile = 0;
            t.IsPartOfZombie = false;
        }

        tilesAroundZombie.Clear();
    }

    private void FindPathToTarget(Grid.Tile targetTile)
    {
        List<Grid.Tile> openSet = new List<Grid.Tile>();
        HashSet<Grid.Tile> closedSet = new HashSet<Grid.Tile>();

        Grid.Tile startTile = gridManager.GetClosest(transform.position);

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

            if (gridManager.IsSameTile(currentTile, targetTile))
            {
                SetWalkBuffer(GetRetracedPathToTargetTile(startTile, targetTile));
                calculatePath = false;
                return;
            }

            List<Grid.Tile> neighbours = GetNeighboursOfTile(currentTile);

            for (int i = 0; i < neighbours.Count; i++)
            {
                if (closedSet.Contains(neighbours[i]) || neighbours[i].Occupied ||
                    tilesAroundZombie.Contains(neighbours[i]))
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
    }

    private bool TileNotReachable(Grid.Tile tile) => tile.IsPartOfZombie;

    private Grid.Tile GetNextTarget()
    {
        if (GamesManager.Instance.GetCollectedBurgers() == GamesManager.Instance.GetBurgerCount())
            return gridManager.GetFinishTile();

        foreach (Grid.Tile tile in gridManager.tiles)
            tile.IsOnPlayerPath = false;

        return gridManager.GetClosest(GamesManager.Instance.BurgersInScene[GamesManager.Instance.GetCollectedBurgers()]
            .transform.position);
    }

    public void OnBurgerCollected() => calculatePath = true;

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
                Grid.Tile tileCorrespondingToNeighbour = gridManager.TryGetTile(tilePosition);
                if (tileCorrespondingToNeighbour != null)
                    neighbours.Add(tileCorrespondingToNeighbour);
            }
        }

        return neighbours;
    }

    Vector3 GetEndPoint()
    {
        return Grid.Instance.WorldPos(Grid.Instance.GetFinishTile());
    }

    GameObject[] GetContextByTag(string aTag)
    {
        Collider[] context = Physics.OverlapSphere(transform.position, ContextRadius);
        List<GameObject> returnContext = new List<GameObject>();
        foreach (Collider c in context)
        {
            if (c.transform.CompareTag(aTag))
            {
                returnContext.Add(c.gameObject);
            }
        }

        return returnContext.ToArray();
    }

    GameObject GetClosest(GameObject[] aContext)
    {
        float dist = float.MaxValue;
        GameObject Closest = null;
        foreach (GameObject z in aContext)
        {
            float curDist = Vector3.Distance(transform.position, z.transform.position);
            if (curDist < dist)
            {
                dist = curDist;
                Closest = z;
            }
        }

        return Closest;
    }
}
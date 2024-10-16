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
    [SerializeField] float ContextRadius;
    [SerializeField] float waitTime;

    private Grid gridManager;

    public override void StartCharacter()
    {
        base.StartCharacter();
        gridManager = Grid.Instance;
        myCurrentTile = gridManager.GetClosest(transform.position);
    }

    public override void UpdateCharacter()
    {
        base.UpdateCharacter();
        Zombie closest = GetClosest(GetContextByTag("Zombie"))?.GetComponent<Zombie>();
        
        FindPathToTarget();
    }

    private void FindPathToTarget()
    {
        List<PathNode> openSet = new List<PathNode>();
        List<PathNode> closedSet = new List<PathNode>();

        PathNode startNode = new PathNode();
        PathNode targetNode = new PathNode();

        startNode.CurrentTile = gridManager.GetClosest(transform.position);
        targetNode.CurrentTile = gridManager.GetFinishTile();
        startNode.CostToMoveToTile = 0;
        startNode.HeuristicCost = MathF.Round(Vector2.Distance(gridManager.WorldPos(myCurrentTile),
            gridManager.WorldPos(targetNode.CurrentTile)), 2);
        openSet.Add(startNode);
            
        while (openSet.Count > 0)
        {
            PathNode currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].TotalCost < currentNode.TotalCost || openSet[i].TotalCost == currentNode.TotalCost)
                    if(openSet[i].HeuristicCost < currentNode.HeuristicCost)
                        currentNode = openSet[i];
            }
            
            openSet.Remove(currentNode);
            
            if(closedSet.Find(x => x.CurrentTile == currentNode.CurrentTile) == null)
                closedSet.Add(currentNode);

            if (gridManager.IsSameTile(currentNode.CurrentTile, targetNode.CurrentTile))
            {
                SetWalkBuffer(GetRetracedPathToTargetNode(startNode, currentNode));
                return;
            }

            List<PathNode> neighbours = GetNeighboursOfNode(currentNode);
            
            for (int i = 0; i < neighbours.Count; i++)
            {
                if (closedSet.Find(x => x.CurrentTile == neighbours[i].CurrentTile) != null || neighbours[i].CurrentTile.occupied)
                    continue;
                
                float newMovementCostToNode = GetDistanceBetweenNodes(currentNode, neighbours[i]);
                
                if (newMovementCostToNode < neighbours[i].CostToMoveToTile || openSet.Find(x => x.CurrentTile == neighbours[i].CurrentTile) == null)
                {
                    neighbours[i].CostToMoveToTile = newMovementCostToNode;
                    neighbours[i].HeuristicCost = GetDistanceBetweenNodes(targetNode, neighbours[i]);
                    neighbours[i].ParentNode = currentNode;
                    
                    if (openSet.Find(x => x.CurrentTile == neighbours[i].CurrentTile) == null)
                        openSet.Add(neighbours[i]);
                }
            }
        }
    }

    private int GetDistanceBetweenNodes(PathNode node1, PathNode node2)
    {
        int distanceX = Mathf.Abs(node1.CurrentTile.x - node2.CurrentTile.x);
        int distanceY = Mathf.Abs(node1.CurrentTile.y - node2.CurrentTile.y);
        
        if(distanceX > distanceY)
            return 14 * distanceY + 10 * (distanceX - distanceY);
        else
            return 14 * distanceX + 10 * (distanceY - distanceX);
    }

    private List<Grid.Tile> GetRetracedPathToTargetNode(PathNode startNode, PathNode currentNode)
    {
        List<Grid.Tile> retracedPath = new List<Grid.Tile>();
        
        while (currentNode != startNode)
        {
            retracedPath.Add(currentNode.CurrentTile);
            currentNode = currentNode.ParentNode;
        }
        retracedPath.Reverse();
        return retracedPath;
    }

    private List<PathNode> GetNeighboursOfNode(PathNode centerNode)
    {
        List<PathNode> neighbours = new List<PathNode>();
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                    continue;

                Vector2Int tilePosition = new Vector2Int(centerNode.CurrentTile.x + i, centerNode.CurrentTile.y + j);
                Grid.Tile tileCorrespondingToNeighbour = gridManager.TryGetTile(tilePosition);
                if (tileCorrespondingToNeighbour != null)
                {
                    PathNode neighbour = new PathNode();
                    neighbour.CurrentTile = tileCorrespondingToNeighbour;
                    neighbours.Add(neighbour);
                }
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
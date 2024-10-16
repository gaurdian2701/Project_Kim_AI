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
    private Random random;
    private float currentWaitStep;
    
    private int frameUpdate = 0;

    public override void StartCharacter()
    {
        base.StartCharacter();
        gridManager = Grid.Instance;
        random = new Random();
        currentWaitStep = waitTime;
        myCurrentTile = gridManager.GetClosest(transform.position);
    }

    public override void UpdateCharacter()
    {
        base.UpdateCharacter();
        Zombie closest = GetClosest(GetContextByTag("Zombie"))?.GetComponent<Zombie>();
        
        FindPathToTarget();
        frameUpdate++;
    }

    private void FindPathToTarget()
    {
        List<PathNode> openSet = new List<PathNode>();
        HashSet<PathNode> closedSet = new HashSet<PathNode>();

        PathNode startNode = new PathNode();
        PathNode targetNode = new PathNode();

        startNode.CurrentTile = gridManager.GetClosest(transform.position);
        startNode.CostToMoveToTile = 0;
        startNode.HeuristicCost = MathF.Round(Vector2.Distance(gridManager.WorldPos(myCurrentTile),
            gridManager.WorldPos(gridManager.GetFinishTile())), 2);
        openSet.Add(startNode);
        
        Debug.Log(openSet[0]);

        targetNode.CurrentTile = gridManager.GetFinishTile();
        int listUpdate = 0;
        while (openSet.Count < 2000 && listUpdate < 60)
        {
            listUpdate++;
            PathNode currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].TotalCost < currentNode.TotalCost || openSet[i].TotalCost == currentNode.TotalCost)
                    if(openSet[i].HeuristicCost < currentNode.HeuristicCost)
                        currentNode = openSet[i];
            }
            
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (gridManager.IsSameTile(currentNode.CurrentTile, targetNode.CurrentTile))
            {
                SetWalkBuffer(GetRetracedPathToTargetNode(startNode, targetNode));
                return;
            }

            List<PathNode> neighbours = GetNeighboursOfNode(currentNode);
            
            for (int i = 0; i < neighbours.Count; i++)
            {
                if (closedSet.Contains(neighbours[i]) || neighbours[i].CurrentTile.occupied)
                {
                    Debug.Log("Contains for closed set working");
                    continue;
                }
                
                float newMovementCostToNode = MathF.Round(currentNode.CostToMoveToTile + Vector2.Distance(gridManager.WorldPos(currentNode.CurrentTile), 
                    gridManager.WorldPos(neighbours[i].CurrentTile)), 2);
                if (newMovementCostToNode < neighbours[i].CostToMoveToTile || !openSet.Contains(neighbours[i]))
                {
                    neighbours[i].CostToMoveToTile = newMovementCostToNode;
                    neighbours[i].HeuristicCost = MathF.Round(Vector2.Distance(gridManager.WorldPos(targetNode.CurrentTile),
                        gridManager.WorldPos(neighbours[i].CurrentTile)), 2);
                    neighbours[i].ParentNode = currentNode;

                    if (!openSet.Contains(neighbours[i]))
                    {
                        openSet.Add(neighbours[i]);
                        GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        c.transform.position = gridManager.WorldPos(neighbours[i].CurrentTile);
                        c.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                        Debug.Log(gridManager.WorldPos(neighbours[i].CurrentTile).magnitude + " " + frameUpdate);
                    }
                }
            }
        }
    }

    private List<Grid.Tile> GetRetracedPathToTargetNode(PathNode startNode, PathNode targetNode)
    {
        List<Grid.Tile> retracedPath = new List<Grid.Tile>();
        PathNode currentNode = targetNode;

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
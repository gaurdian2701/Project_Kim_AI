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
    private List<Grid.Tile> tilesOnCurrentPath;
    private float currentWaitStep;

    private const int minStepsToBeTaken = 5;
    private const int maxStepsToBeTaken = 10;

    public override void StartCharacter()
    {
        base.StartCharacter();
        gridManager = Grid.Instance;
        random = new Random();
        tilesOnCurrentPath = new List<Grid.Tile>();
        currentWaitStep = waitTime;
        myCurrentTile = gridManager.GetClosest(transform.position); 
    }

    private Vector2Int GenerateRandomDirection()
    {
        int direction = random.Next(0, 4);
        switch (direction)
        {
            default:
            case 0:
                return new Vector2Int(0, 1);
            case 1:
                return new Vector2Int(1, 0);
            case 2:
                return new Vector2Int(0, -1);
            case 3:
                return new Vector2Int(-1, 0);
        }
    }

    public override void UpdateCharacter()
    {
        base.UpdateCharacter();
        Zombie closest = GetClosest(GetContextByTag("Zombie"))?.GetComponent<Zombie>();

        if (currentWaitStep <= 0f)
        {
            random = new Random();
            tilesOnCurrentPath.Clear();
            myCurrentTile = gridManager.GetClosest(transform.position);
            int numberOfStepsToBeTaken = random.Next(minStepsToBeTaken, maxStepsToBeTaken + 1);
            Vector2Int tileCalculatedForNextStep = new Vector2Int(myCurrentTile.x, myCurrentTile.y);
            for (int currentStep = 0; currentStep < numberOfStepsToBeTaken; currentStep++)
            {
                Vector2Int nextRandomDirection = GenerateRandomDirection();
                Vector2Int temporaryTileCalculatedForNextStep = tileCalculatedForNextStep;
                temporaryTileCalculatedForNextStep.x += nextRandomDirection.x;
                temporaryTileCalculatedForNextStep.y += nextRandomDirection.y;
                Grid.Tile nextTile = gridManager.TryGetTile(tileCalculatedForNextStep);
                if (nextTile != null)
                {
                    tilesOnCurrentPath.Add(nextTile);
                    tileCalculatedForNextStep = temporaryTileCalculatedForNextStep;
                }
            }
            SetWalkBuffer(tilesOnCurrentPath);
            currentWaitStep = waitTime;
            FindPath();
        }
        else
            currentWaitStep -= Time.deltaTime;
    }

    private void FindPath()
    {
        List<PathNode> openSet = new List<PathNode>();
        List<PathNode> closedSet = new List<PathNode>();
        
        PathNode startNode = new PathNode();
        PathNode currentNode = new PathNode();
        PathNode targetNode = new PathNode();
        
        startNode.CurrentTile = myCurrentTile;
        startNode.CostToMoveToTile = 0;
        startNode.HeuristicCost = Vector2.Distance(gridManager.WorldPos(myCurrentTile), gridManager.WorldPos(gridManager.GetFinishTile()));
        openSet.Add(startNode);

        targetNode.CurrentTile = gridManager.GetFinishTile();

        while (openSet.Count > 0)
        {
            currentNode = openSet[0];
            if (currentNode.CurrentTile == targetNode.CurrentTile)
                return;
            List<PathNode> neighbours = GetNeighboursOfNode(currentNode, targetNode);
        }
    }

    private List<PathNode> GetNeighboursOfNode(PathNode centerNode, PathNode targetNode)
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
                    neighbour.CostToMoveToTile = MathF.Round(Vector2.Distance(gridManager.WorldPos(centerNode.CurrentTile), 
                        gridManager.WorldPos(tileCorrespondingToNeighbour)), 2);
                    neighbour.HeuristicCost = MathF.Round(Vector2.Distance(gridManager.WorldPos(targetNode.CurrentTile),
                        gridManager.WorldPos(tileCorrespondingToNeighbour)), 2);
                    neighbours.Add(neighbour);
                    GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    c.transform.localScale = new Vector3(0.2f, 0.1f, 0.2f);
                    c.transform.position = gridManager.WorldPos(tileCorrespondingToNeighbour);
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

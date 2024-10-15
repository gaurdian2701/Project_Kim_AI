using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class Kim : CharacterController
{
    [SerializeField] float ContextRadius;
    [SerializeField] float waitTime;
    [SerializeField] private List<TileCost> tileCosts = new List<TileCost>();
    
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
        GenerateCosts();
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
        }
        else
            currentWaitStep -= Time.deltaTime;
    }
    
    private void GenerateCosts()
    {
        List<Grid.Tile> allTiles = gridManager.tiles;
        
        for (int i = 0; i < allTiles.Count; i++)
        {
            TileCost tileCost = new TileCost();
            tileCost.Tile = allTiles[i];
            tileCost.CostToMoveToTile =
                Vector3.Distance(gridManager.WorldPos(myCurrentTile), gridManager.WorldPos(allTiles[i]));
            tileCost.CostToMoveToTile = MathF.Round(tileCost.CostToMoveToTile, 2);
            tileCost.HeuristicCost = Vector3.Distance(gridManager.WorldPos(gridManager.GetFinishTile()), gridManager.WorldPos(allTiles[i]));
            tileCost.HeuristicCost = MathF.Round(tileCost.HeuristicCost, 2);
            GameObject c = GameObject.CreatePrimitive(PrimitiveType.Cube);
            c.transform.position = gridManager.WorldPos(allTiles[i]);
            c.transform.localScale = new Vector3(0.2f, 0.1f, 0.2f);
            c.name = tileCost.CostToMoveToTile + " + " + tileCost.HeuristicCost + " = " + tileCost.TotalCost;
            tileCosts.Add(tileCost);
        }
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

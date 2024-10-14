using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class Kim : CharacterController
{
    [SerializeField] float ContextRadius;
    private Grid gridManager;
    private Random random;

    public override void StartCharacter()
    {
        base.StartCharacter();
        
        gridManager = Grid.Instance;
        random = new Random();
        int endingPos = random.Next(2, 10);
        myCurrentTile = gridManager.GetClosest(transform.position);
        List<Grid.Tile> tilesGenerated = new List<Grid.Tile>();
        Vector2Int tileCalculatedForNextStep = new Vector2Int(myCurrentTile.x, myCurrentTile.y);
        for (int i = 1; i < 1000; i++)
        {
            Vector2Int nextRandomDirection = GenerateRandomDirection();
            Vector2Int temporaryTileCalculatedForNextStep = tileCalculatedForNextStep;
            temporaryTileCalculatedForNextStep.x += nextRandomDirection.x;
            temporaryTileCalculatedForNextStep.y += nextRandomDirection.y;
            Grid.Tile nextTile = gridManager.TryGetTile(tileCalculatedForNextStep);
            if (nextTile != null)
            {
                tilesGenerated.Add(nextTile);
                tileCalculatedForNextStep = temporaryTileCalculatedForNextStep;
            }
        }
        SetWalkBuffer(tilesGenerated);
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

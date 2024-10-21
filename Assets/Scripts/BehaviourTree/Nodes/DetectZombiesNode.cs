using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectZombiesNode : Node
{
    private Kim kimScript;
    private List<Grid.Tile> tilesAroundZombie;
    
    public DetectZombiesNode(List<Node> children, Dictionary<string, object> blackboard) : base(children,
        blackboard)
    {
        kimScript = (Kim)blackboard["KimScript"];
        tilesAroundZombie = blackboard["TilesAroundZombie"] as List<Grid.Tile>;
    }

    public override NodeStates Evaluate()
    {
        Zombie closestZombie = kimScript.GetClosest(kimScript.GetContextByTag("Zombie"))?.GetComponent<Zombie>();

        if (closestZombie != null)
        {
            GetTileCostsAroundZombie(closestZombie);

            if (kimScript.GetNextTarget().IsPartOfZombie)
            {
                if (Vector3.Distance(kimScript.gameObject.transform.position, closestZombie.transform.position) >
                    (float)myBlackboard["ContextRadius"])
                {
                    kimScript.ClearWalkBuffer();
                    return NodeStates.FAILURE;
                }
            }
            
            return NodeStates.SUCCESS;
        }

        return NodeStates.FAILURE;
    }
    
    private void GetTileCostsAroundZombie(Zombie closestZombie)
    {
        Grid.Tile centerTile = Grid.Instance.GetClosest(closestZombie.transform.position);
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

                Grid.Tile tileReturned = Grid.Instance.TryGetTile(new Vector2Int(centerTile.x + i, centerTile.y + j));
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
}

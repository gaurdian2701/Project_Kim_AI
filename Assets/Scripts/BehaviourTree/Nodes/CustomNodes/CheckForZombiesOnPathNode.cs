using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckForZombiesOnPathNode : Node
{
    private Kim kimScript;
    private List<Grid.Tile> tilesAroundZombie;
    
    public CheckForZombiesOnPathNode(List<Node> children, Dictionary<string, object> blackboard) : base(children,
        blackboard)
    {
        kimScript = blackboard["KimScript"] as Kim;
        tilesAroundZombie = blackboard["TilesAroundZombie"] as List<Grid.Tile>;
    }

    public override NodeStates Evaluate()
    {
        Zombie closestZombie = kimScript.GetClosest(kimScript.GetContextByTag("Zombie"))?.GetComponent<Zombie>();
        myBlackboard["PlayerIsWaitingForOpening"] = false;

        if (closestZombie != null)
        {
            Debug.Log("ZOMBIE FOUND");
            GetTileCostsAroundZombie(closestZombie);
            myBlackboard["ClosestZombie"] = closestZombie;
            myBlackboard["CalculatePath"] = true;
            return NodeStates.SUCCESS;
        }
        ClearZombieData();

        if ((bool)myBlackboard["PlayerIsWaitingForOpening"])
            myBlackboard["CalculatePath"] = true;
        
        myBlackboard["ClosestZombie"] = null;
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
        List<Grid.Tile> tilesAroundZombie = myBlackboard["TilesAroundZombie"] as List<Grid.Tile>;
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

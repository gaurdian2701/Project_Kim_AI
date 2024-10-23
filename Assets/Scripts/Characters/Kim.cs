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
    public bool BurgerCollected = false;
    
    private KimBehaviourTree kimTree;
    private Grid gridManager;
    private bool calculatePath = true;
    private List<Grid.Tile> tilesAroundZombie = new List<Grid.Tile>();
    private Zombie closestZombie;

    public override void StartCharacter()
    {
        base.StartCharacter();
        gridManager = Grid.Instance;
        kimTree = new KimBehaviourTree(this);
        myCurrentTile = gridManager.GetClosest(transform.position);
    }

    public override void UpdateCharacter()
    {
        base.UpdateCharacter();
        kimTree.Update();
    }

    public Grid.Tile GetNextTarget()
    {
        if (GamesManager.Instance.GetCollectedBurgers() == GamesManager.Instance.GetBurgerCount())
            return gridManager.GetFinishTile();

        foreach (Grid.Tile tile in gridManager.tiles)
            tile.IsOnPlayerPath = false;

        return gridManager.GetClosest(GamesManager.Instance.BurgersInScene[GamesManager.Instance.GetCollectedBurgers()]
            .transform.position);
    }

    public void OnBurgerCollected()
    {
        BurgerCollected = true;
        calculatePath = true;
    }

    Vector3 GetEndPoint()
    {
        return Grid.Instance.WorldPos(Grid.Instance.GetFinishTile());
    }

    public GameObject[] GetContextByTag(string aTag)
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

    public GameObject GetClosest(GameObject[] aContext)
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
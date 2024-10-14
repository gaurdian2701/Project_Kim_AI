using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//#     # #     #  #####  #     #    ######  ######     #    ### #     #  #####  
//#     # #     # #     # #     #    #     # #     #   # #    #  ##    # #     # 
//#     # #     # #       #     #    #     # #     #  #   #   #  # #   # #       
//#     # #     # #  #### #######    ######  ######  #     #  #  #  #  #  #####  
//#     # #     # #     # #     #    #     # #   #   #######  #  #   # #       # 
//#     # #     # #     # #     #    #     # #    #  #     #  #  #    ## #     # 
// #####   #####   #####  #     #    ######  #     # #     # ### #     #  #####  

public class Zombie : CharacterController
{
    float currentIdleTime = 0;
    Vector2 idleWaitTimeRange = new Vector2(5, 8);
    Vector2Int randomDirectionRange = new Vector2Int(3, 7);

    public override void StartCharacter()
    {
        base.StartCharacter();
    }

    public override void UpdateCharacter()
    {
        base.UpdateCharacter();

        if (currentIdleTime > 0)
        {
            currentIdleTime -= Time.deltaTime;
        }
        else
        {
            LetGodDecideNextDestination();
        }
    }

    public void LetGodDecideNextDestination()
    {
        currentIdleTime = Random.Range(idleWaitTimeRange.x, idleWaitTimeRange.y);
        LetGodCalculateNextDestination();
    }

    public void LetGodCalculateNextDestination()
    {
        int direction = Random.Range(0, 4);
        int destinationOffset = Random.Range(randomDirectionRange.x, randomDirectionRange.y);

        List<Grid.Tile> pathOfTilesTaken = new List<Grid.Tile>();

        Vector2Int stepTakenInADirection = Vector2Int.zero;

        switch (direction)
        {
            case 0:
                stepTakenInADirection = new Vector2Int(1, 0);
                break;
            case 1:
                stepTakenInADirection = new Vector2Int(-1, 0);
                break;
            case 2:
                stepTakenInADirection = new Vector2Int(0, 1);
                break;
            case 3:
                stepTakenInADirection = new Vector2Int(0, -1);
                break;
        }

        for (int offsetToNextTile = 1; offsetToNextTile < destinationOffset + 1; offsetToNextTile++)
        {
            Grid.Tile nextTileCalculated = Grid.Instance.TryGetTile(new Vector2Int(myCurrentTile.x + (stepTakenInADirection.x * offsetToNextTile),
                myCurrentTile.y + (stepTakenInADirection.y * offsetToNextTile)));
            if (nextTileCalculated != null)
            {
                if (!nextTileCalculated.occupied)
                {
                    pathOfTilesTaken.Add(nextTileCalculated);
                }
            }
        }

        SetWalkBuffer(pathOfTilesTaken);
    }
}
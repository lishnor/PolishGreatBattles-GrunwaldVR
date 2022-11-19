using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public IEnumerator TurnIterator() 
    {
        //Begin Game

        //Loop
        //if player only left -> player win
        //if enemy only left -> player loose
        //Do playerturn
        //Do enemyTurn
        yield return null;
    }

    public IEnumerator PlayerTurn()
    {
        //make move on board
        //move people on board //if not destined to merge do not setup obstacle  except the one moving
        //PlayEnemy turn //always obstacle except the one moving;
        yield return null;
    }

    public IEnumerator EnemyTurn() 
    {
        //Find distances between Crussaders and Poles
        //Find Strongest Army That exceds poles
        //Move that army to poles
        yield return null;
    }
}

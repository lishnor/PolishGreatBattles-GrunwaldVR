using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private BattleUnitsManager _battleUnitsManager;

    private void Start()
    {
        StartCoroutine(TurnIterator());
    }

    public IEnumerator TurnIterator() 
    {
        //Begin Game

        //Loop
        while (!_battleUnitsManager.IsEndState) 
        {
            //Do playerturn
            yield return PlayerTurn();
            //Do enemyTurn
            yield return EnemyTurn(); 
        }

        //if player only left -> player win
        //if enemy only left -> player loose
        if (_battleUnitsManager.EnemiesCount <= 0)
        {
            yield return Win();
        }
        else 
        {
            yield return Loose();
        }
    }

    public IEnumerator PlayerTurn()
    {
        //UnlockInteractable
        //make move on board
        //wait for order
        //move people on board //if not destined to merge do not setup obstacle  except the one moving

        Debug.Log("Player Turn");
        yield return _battleUnitsManager.PlayerMove();
        yield return new WaitForSeconds(0.1f);
    }

    public IEnumerator EnemyTurn() 
    {
        Debug.Log("Enemy Turn");
        //Find distances between Crussaders and Poles
        //Find Strongest Army That exceds poles
        //Move that army to poles
        //always obstacle except the one moving;
        yield return _battleUnitsManager.EnemyMove();
        yield return new WaitForSeconds(0.1f);
    }

    public IEnumerator Win() 
    {
        Debug.Log("Win");
        yield return null;
    }

    public IEnumerator Loose() 
    {
        Debug.Log("Loose");
        yield return null;
    }
}

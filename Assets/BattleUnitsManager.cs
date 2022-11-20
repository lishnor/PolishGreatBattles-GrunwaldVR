using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.XR.OpenXR.Input;
using System;
using static UnityEngine.GraphicsBuffer;

public class BattleUnitsManager : MonoBehaviour
{
    [SerializeField] private Transform _board;
    [SerializeField] private Transform _battlefield;
    [SerializeField] private float _battlefieldToBoardScale = 10;

    [SerializeField] private GamePiece _polePieceToSpawn;
    [SerializeField] private GamePiece _enemyPieceToSpawn;


    [SerializeField] private List<GamePieceToBattleUnit> Poles = new List<GamePieceToBattleUnit>();
    public int PolesCount => Poles.Count;
    [SerializeField] private List<GamePieceToBattleUnit> Enemies = new List<GamePieceToBattleUnit>();
    public int EnemiesCount => Enemies.Count;

    public bool IsEndState => PolesCount <= 0 || EnemiesCount <= 0;
    private bool _playerMoveEnded = false;
    private BattleUnit _currentMovingPlayerBattleUnit;
    private bool _playerCommandSended = false;

    private void OnEnable()
    {
        GetInitialBattleUnitsInWorld();
    }

    private void GetInitialBattleUnitsInWorld() 
    {
        var allBattleUnits = FindObjectsOfType<BattleUnit>().ToList();

        //Find All Poles
        var poles = allBattleUnits.Where(x => x.BattleSide == BattleSide.Poles).ToList();

        //find all Enemies
        var enemies = allBattleUnits.Where(x => x.BattleSide == BattleSide.Crusaders).ToList();


        //CreatePlayerGamePieces
        foreach (var pole in poles)
        {
            CreatePlayerGamePieces(pole, _polePieceToSpawn, Poles);
        }

        //CreateEnemyGamePieces
        foreach (var enemy in enemies)
        {
            CreatePlayerGamePieces(enemy, _enemyPieceToSpawn, Enemies);
        }
    }

    private void CreatePlayerGamePieces(BattleUnit unit, GamePiece pieceToSpawn, List<GamePieceToBattleUnit> pieceToUnits)
    {
        var localPositionInBattleField = _battlefield.InverseTransformPoint(unit.transform.position);
        localPositionInBattleField.y = 0;
        localPositionInBattleField /= _battlefieldToBoardScale;

        var piece = Instantiate(pieceToSpawn, _board);
        piece.transform.localPosition = localPositionInBattleField;
        piece.AssignBattleUnit(unit, _battlefield, _battlefieldToBoardScale);
        var pieceToUnit = new GamePieceToBattleUnit(piece, unit);
        pieceToUnits.Add(pieceToUnit);

        if (!piece.IsAIDriven) 
        {
            piece.OnMoveEnded += ctx => 
            {
                _playerMoveEnded = true; 
                
            };
            piece.OnMoveCommandExecute += () => _playerCommandSended = true;
        }

        unit.OnBattleUnitDestroyed += () =>
        {
            pieceToUnits.Remove(pieceToUnit);
            Destroy(piece.gameObject);
        };
    }

    public IEnumerator PlayerMove() 
    {
        //UnlockInteractable
        _playerMoveEnded = false;
        foreach (var pole in Poles) 
        {
            pole.GamePiece.UnlockInteractable();
        }
        //make move on board
        while (!_playerMoveEnded) 
        {
            yield return new WaitForSeconds(0.1f);
        }

        foreach (var pole in Poles)
        {
            pole.GamePiece.LockInteractable();
        }

        //wait for order
        while (!_playerCommandSended)
        {
            yield return new WaitForSeconds(0.1f);
        }

        while (_currentMovingPlayerBattleUnit.IsUpdating)
        {
            yield return new WaitForSeconds(0.1f);
        }
    }

    public IEnumerator EnemyMove() 
    {
        List<EnemyTargeting> enemyAndDisnance = new List<EnemyTargeting>();
        foreach (var enemy in Enemies) 
        {
            var enemyTargeting = new EnemyTargeting() { Enemy = enemy };
            var shortestDistance = Mathf.Infinity;
            foreach (var pole in Poles) 
            {
                if (enemy.BattleUnit.PeopleCount > pole.BattleUnit.PeopleCount) 
                {
                    var distance = Vector3.Distance(enemy.BattleUnit.transform.position,pole.BattleUnit.transform.position);
                    shortestDistance = distance < shortestDistance ? distance : shortestDistance;
                    enemyTargeting.Pole= pole;
                    enemyTargeting.distance = shortestDistance; 
                }
            }

            enemyTargeting.Pole ??= Poles[0];
            
            enemyAndDisnance.Add(enemyTargeting);
        }

        enemyAndDisnance = enemyAndDisnance.OrderByDescending(x => x.distance).ToList();

        var target = enemyAndDisnance[0];

        for (int i = 1; i < enemyAndDisnance.Count; i++)
        {
            enemyAndDisnance[i].Enemy.BattleUnit.ActivateObstacle();
        }

        target.Enemy.BattleUnit.DeactivateObstacle();
        target.Enemy.BattleUnit.MoveToPositon(target.Pole.BattleUnit.transform.position);

        while (target.Enemy.BattleUnit.IsUpdating) 
        {
            yield return new WaitForSeconds(0.1f);
        }
    }

    class EnemyTargeting 
    {
        public GamePieceToBattleUnit Enemy;
        public GamePieceToBattleUnit Pole;
        public float distance = Mathf.Infinity;
    }
}

public class GamePieceToBattleUnit 
{
    public GamePiece GamePiece;
    public BattleUnit BattleUnit;

    public GamePieceToBattleUnit(GamePiece piece, BattleUnit unit)
    {
        GamePiece = piece;
        BattleUnit = unit;
    }
}

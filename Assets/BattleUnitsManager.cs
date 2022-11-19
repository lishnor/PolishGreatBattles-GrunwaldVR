using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.XR.OpenXR.Input;

public class BattleUnitsManager : MonoBehaviour
{
    [SerializeField] private Transform _board;
    [SerializeField] private Transform _battlefield;
    [SerializeField] private float _battlefieldToBoardScale = 10;

    [SerializeField] private GamePiece _polePieceToSpawn;
    [SerializeField] private GamePiece _enemyPieceToSpawn;


    [SerializeField] private List<GamePieceToBattleUnit> Poles = new List<GamePieceToBattleUnit>();
    [SerializeField] private List<GamePieceToBattleUnit> Enemies = new List<GamePieceToBattleUnit>();

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
            var localPositionInBattleField = _battlefield.InverseTransformPoint(pole.transform.position);
            localPositionInBattleField.y = 0;
            localPositionInBattleField /= _battlefieldToBoardScale;

            var polePiece = Instantiate(_polePieceToSpawn, _board);
            polePiece.transform.localPosition = localPositionInBattleField;
            polePiece.AssignBattleUnit(pole);
            var polePieceToUnit = new GamePieceToBattleUnit(polePiece, pole);
            Poles.Add(polePieceToUnit);
        }

        //CreateEnemyGamePieces
        foreach (var enemy in enemies)
        {
            var localPositionInBattleField = _battlefield.InverseTransformPoint(enemy.transform.position);
            localPositionInBattleField.y = 0;
            localPositionInBattleField /= _battlefieldToBoardScale;

            var enemyPiece = Instantiate(_enemyPieceToSpawn, _board);
            enemyPiece.transform.localPosition = localPositionInBattleField;
            enemyPiece.AssignBattleUnit(enemy);
            var enemyPieceToUnit = new GamePieceToBattleUnit(enemyPiece, enemy);
            Enemies.Add(enemyPieceToUnit);
        }
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

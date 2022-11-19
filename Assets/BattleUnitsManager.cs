using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.XR.OpenXR.Input;
using System;

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

        var piece = Instantiate(_polePieceToSpawn, _board);
        piece.transform.localPosition = localPositionInBattleField;
        piece.AssignBattleUnit(unit);
        var pieceToUnit = new GamePieceToBattleUnit(piece, unit);
        pieceToUnits.Add(pieceToUnit);

        unit.OnBattleUnitDestroyed += () =>
        {
            pieceToUnits.Remove(pieceToUnit);
            Destroy(piece.gameObject);
        };
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

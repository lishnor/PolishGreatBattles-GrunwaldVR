using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class GamePiece : MonoBehaviour
{
    [field: SerializeField] public BattleSide BattleSide { get; private set; }
    private BattleUnit _battleUnit;

    [SerializeField] private TMP_Text _countDisplay;
    

    public void AssignBattleUnit(BattleUnit battleUnit) 
    {
        _battleUnit = battleUnit;
        _battleUnit.OnUpdateCount += UpdateDisplay;
    }

    private void UpdateDisplay(int value)
    {
        _countDisplay.text = value.ToString();
    }

    [ContextMenu("PerformMove")]
    private void TestMove() 
    {
        var localPos = transform.localPosition;
        var targetPos = localPos * 10f;
        targetPos.y = 0;
        _battleUnit.MoveToPositon(targetPos);
    }
}

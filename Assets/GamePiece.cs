using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePiece : MonoBehaviour
{
    [field: SerializeField] public BattleSide BattleSide { get; private set; }
    private BattleUnit _battleUnit;

    public void AssignBattleUnit(BattleUnit battleUnit) 
    {
        _battleUnit = battleUnit;
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

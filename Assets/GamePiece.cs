using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.Events;

public class GamePiece : MonoBehaviour
{
    [field: SerializeField] public BattleSide BattleSide { get; private set; }
    private BattleUnit _battleUnit;

    [SerializeField] private TMP_Text _countDisplay;
    [SerializeField] private bool _isAIDriven = false;
    public UnityAction OnMoveEnded;

    private Transform _batlleField = null;
    private float _scale = 10f;

    public void AssignBattleUnit(BattleUnit battleUnit, Transform battlefield, float scale) 
    {
        _battleUnit = battleUnit;
        _battleUnit.OnUpdateCount += UpdateDisplay;
        _batlleField = battlefield;
        _scale = scale;

        if (_isAIDriven)
        {
            _battleUnit.OnUpdatePosition += MovePieceAI;
        }
    }

    private void MovePieceAI()
    {
         var localPos = _batlleField.InverseTransformPoint(_batlleField.position);
        localPos /= _scale;
        localPos.y = 0;
        transform.localPosition= localPos;
    }

    private void UpdateDisplay(int value) => _countDisplay.text = value.ToString();


    [ContextMenu("PerformMove")]
    private void MovePiece() 
    {
        var localPos = transform.localPosition;
        var targetPos = localPos * _scale;
        targetPos.y = 0;
        targetPos = _batlleField.TransformPoint(targetPos);
        OnMoveEnded?.Invoke();
        _battleUnit.MoveToPositon(targetPos);
    }
}

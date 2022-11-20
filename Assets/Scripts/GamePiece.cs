using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class GamePiece : MonoBehaviour
{
    [field: SerializeField] public BattleSide BattleSide { get; private set; }
    private BattleUnit _battleUnit;

    [SerializeField] private TMP_Text _countDisplay;
    [SerializeField] private bool _isAIDriven = false;
    public bool IsAIDriven => _isAIDriven;

    [field: SerializeField] public XRGrabInteractable GrabInteractable { get; private set; }
    [SerializeField] private Rigidbody _rigidbody;
    public UnityAction<BattleUnit> OnMoveEnded;
    public UnityAction OnMoveCommandExecute;

    private Transform _board;
    private Transform _batlleField = null;
    private float _scale = 10f;

    private Vector3 _startInteractionPosition = Vector3.zero;


    public void AssignBattleUnit(BattleUnit battleUnit, Transform battlefield, float scale) 
    {
        _board = transform.parent;
        _battleUnit = battleUnit;
        _battleUnit.OnUpdateCount += UpdateDisplay;
        _batlleField = battlefield;
        _scale = scale;

        if (_isAIDriven)
        {
            _battleUnit.OnUpdatePosition += MovePieceAI;
        }
        else 
        {
            GrabInteractable?.selectEntered.AddListener(StartInteracting);
            GrabInteractable?.selectExited.AddListener(StopInteracting);
        }
    }

    [ContextMenu("EndInteraction")]
    private void DebugInteractionEnd() => StopInteracting(null);

    private void StopInteracting(SelectExitEventArgs arg0)
    {
        transform.parent = _board;
        DistanceCircle.Instance.DeactivateCircle();
        transform.localEulerAngles = Vector3.zero;
        var position = _board.InverseTransformPoint(transform.position);
        position.y = 0;
        var offset = position - _startInteractionPosition;
        offset = Vector3.ClampMagnitude(offset, 0.25f);
        transform.localPosition = _startInteractionPosition + offset;
        MovePiece();
    }

    [ContextMenu("StartInteraction")]
    private void DebugInteractionStart() => StartInteracting(null);

    private void StartInteracting(SelectEnterEventArgs arg0)
    {
        _startInteractionPosition = _board.InverseTransformPoint(transform.position);
        DistanceCircle.Instance.ActivateCircle(_startInteractionPosition);
    }

    public void LockInteractable() 
    {
        _rigidbody.isKinematic = true;
        GrabInteractable.enabled = false;
    }

    public void UnlockInteractable() 
    {
        _rigidbody.isKinematic = false;
        GrabInteractable.enabled = true;
    }

    private void MovePieceAI()
    {
         var localPos = _batlleField.InverseTransformPoint(_battleUnit.transform.position);
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
        OnMoveEnded?.Invoke(_battleUnit);
        _battleUnit.MoveToPositon(targetPos);
        OnMoveCommandExecute?.Invoke();
        Debug.Log("IAmhere");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HandAnimator : MonoBehaviour
{
    [SerializeField] private InputActionProperty _grip;
    [SerializeField] private Animator _animator;

    int _gripIndex = 0;

    private void Start()
    {
        _gripIndex = Animator.StringToHash("Fist");
    }

    private void Update()
    {
        var value = _grip.action.ReadValue<float>();
        _animator.SetFloat(_gripIndex, value);
    }
}

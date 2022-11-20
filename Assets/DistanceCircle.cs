using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceCircle : MonoBehaviour
{
    public static DistanceCircle Instance { get; private set; }

    [SerializeField] private MeshRenderer _renderer;

    private void Awake()
    {
        Instance ??= this;
        DeactivateCircle();
    }

    public void ActivateCircle(Vector3 localPosition) 
    {

        var targetPos = localPosition;
        targetPos.y = 0.001f;
        transform.localPosition = targetPos;
        _renderer.enabled = true;
    }

    public void DeactivateCircle() 
    {
        _renderer.enabled = false;
    }
}

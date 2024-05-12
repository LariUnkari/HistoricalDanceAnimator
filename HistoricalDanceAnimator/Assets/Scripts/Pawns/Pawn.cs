using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : MonoBehaviour
{
    public DancerPosition _dancerPosition;

    private Vector3 _dancerAnimationPosition;
    private Vector3 _dancerAnimationDirection;
    private Quaternion _dancerAnimationRotation;

    private void LateUpdate()
    {
        if (!Application.isPlaying || _dancerPosition == null) { return; }

        _dancerAnimationPosition = _dancerPosition.GetPosition();
        _dancerAnimationDirection = _dancerPosition.GetDirection();
        _dancerAnimationRotation = _dancerPosition.GetRotation();

        transform.position = _dancerAnimationPosition;
        transform.rotation = _dancerAnimationRotation;
    }
}

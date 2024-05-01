using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : MonoBehaviour
{
    public DancerPosition _dancerPosition;

    private Vector3 _dancerAnimationPosition;

    private void LateUpdate()
    {
        if (!Application.isPlaying || _dancerPosition == null) { return; }

        _dancerAnimationPosition = _dancerPosition.GetPosition();
        transform.position = _dancerAnimationPosition;
    }
}

using UnityEngine;

public class Pawn : MonoBehaviour
{
    public PawnModel model;
    public DancerPosition _dancerPosition;

    private Vector3 renderOffset;
    private Vector3 _dancerAnimationPosition;
    private Vector3 _dancerAnimationDirection;
    private Quaternion _dancerAnimationRotation;
    private Vector3 _dancerAnimationScale;

    private void LateUpdate()
    {
        if (!Application.isPlaying || _dancerPosition == null) { return; }

        _dancerAnimationPosition = _dancerPosition.GetPosition();
        _dancerAnimationDirection = _dancerPosition.GetDirection();
        _dancerAnimationRotation = _dancerPosition.GetRotation();
        _dancerAnimationScale = _dancerPosition.GetScale();

        transform.position = _dancerAnimationPosition + renderOffset;
        transform.rotation = _dancerAnimationRotation;
        transform.localScale = _dancerAnimationScale;
    }

    public void SetDancerPosition(DancerPosition dancerPosition)
    {
        _dancerPosition = dancerPosition;
        SetRenderOffset(dancerPosition.Role.renderOffset);
    }

    private void SetRenderOffset(float offset)
    {
        renderOffset = Vector3.forward * offset;
    }
}

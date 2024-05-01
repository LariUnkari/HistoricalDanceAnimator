using System.Collections;
using UnityEngine;

public class DancerPosition : MonoBehaviour
{
    /// <summary>
    /// Transform moved by dance action animations,
    /// used to calculate dancer pawn position.
    /// </summary>
    public Transform _dancer;

    /// <summary>
    /// Animation component to run dance animations animations on.
    /// </summary>
    public Animation _animation;

    private DancerRole _role;

    private int _beatIndex;
    private DanceAction _currentDanceAction;
    private DanceAction _nextDanceAction;

    private IEnumerator _danceActionRoutine;

    public void Init()
    {
        _dancer = new GameObject("Dancer").transform;
        _dancer.parent = transform;

        _animation = gameObject.AddComponent<Animation>();

        ActionPreset idlePreset;
        if (ActionPresetDatabase.GetInstance().TryGetPreset("Idle", out idlePreset))
        {
            _animation.AddClip(idlePreset.animation, idlePreset.action);
            _animation.Play(idlePreset.action);
        }
        //else
        //{
        //    Debug.LogWarning(name + ": Unable to get idle animation");
        //}
    }

    public void SetRole(DancerRole role)
    {
        _role = role;
    }

    public void BeginDance()
    {
        _beatIndex = -1;
    }

    public void DanceUpdate(float danceTime, float beatTime, float beatDuration, int beatIndex)
    {
        if (beatIndex == _beatIndex)
            return;

        _beatIndex = beatIndex;

        if (_role.TryGetAction(_beatIndex, out _nextDanceAction))
        {
            if (_currentDanceAction != null && _nextDanceAction != _currentDanceAction)
                OnDanceActionCompleted(_currentDanceAction);

            _currentDanceAction = _nextDanceAction;
            PlayDanceAction(_currentDanceAction, beatDuration);
        }
        //else
        //{
        //    Debug.LogError($"{name}: Unable to get DanceAction at beat {_beatIndex}");
        //}
    }

    public void PlayDanceAction(DanceAction danceAction, float beatDuration)
    {
        Debug.Log($"{_role.group.id}.{_role.id}: Beat[{_beatIndex}]: Play dance action {danceAction.actionName}.{danceAction.variantName}'" +
            $", movement: {danceAction.movement}, action.duration={danceAction.duration:F3} at beatDuration={beatDuration:F3}");

        if (_danceActionRoutine != null)
        {
            Debug.LogWarning("Stopping old dance action routine");
            StopCoroutine(_danceActionRoutine);
        }

        _danceActionRoutine = DanceActionRoutine(danceAction, beatDuration);
        StartCoroutine(_danceActionRoutine);
    }

    private IEnumerator DanceActionRoutine(DanceAction danceAction, float beatDuration)
    {
        float timeScale = danceAction.duration * beatDuration;
        Debug.Log($"{_role.group.id}.{_role.id}: Beat[{_beatIndex}]: Starting to play dance action {danceAction.actionName}.{danceAction.variantName}'" +
            $", movement: {danceAction.movement}, action.duration={danceAction.duration:F3} at beatDuration={beatDuration:F3}, timeScale={timeScale:F3}");

        _animation.AddClip(danceAction.animationClip, danceAction.animationClip.name);
        foreach (AnimationState state in _animation) { state.speed = timeScale; }
        _animation.Play(danceAction.animationClip.name);

        while (_animation.isPlaying)
            yield return null;

        _danceActionRoutine = null;
        OnDanceActionCompleted(danceAction);
    }

    private void OnDanceActionCompleted(DanceAction danceAction)
    {
        Debug.Log($"{_role.group.id}.{_role.id}: Beat[{_beatIndex}]: Finished playing dance action '{danceAction.actionName}.{danceAction.variantName}', movement: {danceAction.movement}");

        transform.position = GetPosition();
        _dancer.localPosition = Vector3.zero;
    }

    public Vector3 GetPosition()
    {
        if (_dancer == null || _currentDanceAction == null || _currentDanceAction.movement == null || _currentDanceAction.movement.directions == null)
            return transform.position;

        if (_currentDanceAction.movement.directions.Length > 0)
        {
            DanceVector vector = _currentDanceAction.movement.directions[0]; // TODO: Only first direction taken here, should take all as a compound and normalize it
            return transform.TransformPoint(OrientateVector(_dancer.localPosition, vector));
        }

        return transform.TransformPoint(_dancer.localPosition);
    }

    public Vector3 OrientateVector(Vector3 localVector, DanceVector danceVector)
    {
        return Quaternion.FromToRotation(Vector3.up, GetVectorFromDirection(danceVector.direction)) * localVector * danceVector.distance;
    }

    public Vector3 GetVectorFromDirection(DanceDirection danceDirection)
    {
        switch (danceDirection)
        {
            case DanceDirection.Down:  return Vector3.down;
            case DanceDirection.Left:  return Vector3.left;
            case DanceDirection.Right: return Vector3.right;
            default: break;
        }

        return Vector3.up;
    }
}

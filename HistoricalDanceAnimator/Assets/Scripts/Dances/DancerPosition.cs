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

        foreach (DanceAction action in role.GetActionCollection())
            _animation.AddClip(action.animationClip, action.animationClip.name);
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
            PlayDanceAction(_currentDanceAction, beatTime, beatDuration);
        }
        //else
        //{
        //    Debug.LogError($"{name}: Unable to get DanceAction at beat {_beatIndex}");
        //}
    }

    public void PlayDanceAction(DanceAction danceAction, float beatTime, float beatDuration)
    {
        Debug.Log($"{_role.group.id}.{_role.id}: Beat[{_beatIndex}] time={beatTime:F3}: Play dance action {danceAction.actionName}.{danceAction.variantName}'" +
            $", movement: {danceAction.movement}, action.duration={danceAction.duration:F3} at beatDuration={beatDuration:F3}");

        if (_danceActionRoutine != null)
        {
            Debug.LogWarning("Stopping old dance action routine");
            StopCoroutine(_danceActionRoutine);
        }

        transform.rotation = Quaternion.FromToRotation(Vector3.up, GetVectorFromDirection(danceAction.startFacing));

        _danceActionRoutine = DanceActionRoutine(danceAction, beatTime, beatDuration);
        StartCoroutine(_danceActionRoutine);
    }

    private IEnumerator DanceActionRoutine(DanceAction danceAction, float beatTime, float beatDuration)
    {
        float timeScale = (danceAction.animationDuration / beatDuration) / danceAction.duration;

        Debug.Log($"{_role.group.id}.{_role.id}: Beat[{_beatIndex}] time={beatTime:F3}: Starting to play dance action {danceAction.actionName}.{danceAction.variantName}', movement: {danceAction.movement}, animation: '{danceAction.animationClip.name}'\n"+
            $"action.duration={danceAction.duration}, action.animationDuration={danceAction.animationDuration} at beatDuration={beatDuration:F3}, timeScale={timeScale:F3}");

        // Restart animation at correct time if it was already playing
        if (_animation.IsPlaying(danceAction.animationClip.name))
            _animation[danceAction.animationClip.name].time = beatTime; 
        else
            _animation.Play(danceAction.animationClip.name);

        foreach (AnimationState state in _animation) { state.speed = timeScale; }

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

    public Vector3 GetDirection()
    {
        return _dancer.up;
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

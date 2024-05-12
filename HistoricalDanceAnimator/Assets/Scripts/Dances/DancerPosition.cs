using System.Collections;
using UnityEngine;

public class DancerPosition : MonoBehaviour
{
    public bool _doDebug;

    /// <summary>
    /// Transform moved by dance action animations,
    /// used to calculate dancer pawn position.
    /// </summary>
    public Transform _dancer;

    /// <summary>
    /// Animation component to run dance animations animations on.
    /// </summary>
    public Animation _animation;

    private DanceFormation _formation;
    private DancerRole _role;

    private int _beatIndex;
    private float _beatT;
    private float _beatTime;
    private float _danceTime;

    private float _actionT;
    private float _actionTime;

    private bool _isTransitioning;
    private bool _isTransitionComplete;
    private float _transitionT;
    private float _transitionAmount;

    [HideInInspector] private DanceAction _currentDanceAction;
    [HideInInspector] private DanceAction _nextDanceAction;

    private IEnumerator _danceActionRoutine;

    public void Init(DanceFormation formation)
    {
        _formation = formation;

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

    public void DanceUpdate(float danceTime, int beatIndex, float beatTime, float beatT, float beatDuration)
    {
        _beatT = beatT;
        _beatTime = beatTime;
        _danceTime = danceTime;

        if (beatIndex != _beatIndex)
        {
            _beatIndex = beatIndex;

            if (_role.TryGetAction(_beatIndex, out _nextDanceAction))
            {
                if (_currentDanceAction != null && _nextDanceAction != _currentDanceAction)
                    OnDanceActionCompleted(_currentDanceAction);

                PlayDanceAction(_nextDanceAction, beatTime, beatT, beatDuration);
            }
            //else
            //{
            //    Debug.LogError($"{name}: Unable to get DanceAction at beat {_beatIndex}");
            //}
        }

        if (_currentDanceAction != null)
        {
            _actionTime = _danceTime - _currentDanceAction.time * beatDuration;
            _actionT = _actionTime / (_currentDanceAction.duration * beatDuration);

            if (_currentDanceAction.transition != null)
            {
                if (_isTransitioning)
                {
                    if (_actionT >= _currentDanceAction.transition.time + _currentDanceAction.transition.duration)
                    {
                        _isTransitioning = false;
                        _isTransitionComplete = true;
                        _transitionT = 1f;
                        _transitionAmount = _currentDanceAction.transition.amount;

                        if (_doDebug)
                            Debug.Log($"Done Transitioning: amount={_transitionAmount}");
                    }
                    else
                    {
                        _transitionT = (_actionT - _currentDanceAction.transition.time) / _currentDanceAction.transition.duration;
                        _transitionAmount = _transitionT * _currentDanceAction.transition.amount;

                        if (_doDebug)
                            Debug.Log($"Transitioning: actionT={_actionT} transitionT={_transitionT} amount={_transitionAmount}");
                    }
                }
                else
                {
                    if (_actionT > _currentDanceAction.transition.time && _actionT < _currentDanceAction.transition.time + _currentDanceAction.transition.duration)
                    {
                        _isTransitioning = true;
                        _transitionT = (_actionT - _currentDanceAction.transition.time) / _currentDanceAction.transition.duration;
                        _transitionAmount = _transitionT * _currentDanceAction.transition.amount;

                        if (_doDebug)
                            Debug.Log($"Started transitioning: actionT={_actionT} transitionT={_transitionT} amount={_transitionAmount}");
                    }
                }
            }
        }
    }

    public void PlayDanceAction(DanceAction danceAction, float beatTime, float beatT, float beatDuration)
    {
        if (danceAction == null)
        {
            Debug.LogError($"{_role.group.id}.{_role.id}: Beat[{_beatIndex}] t={beatT:F3} time={beatTime:F3}: Can't play NULL dance action");
            return;
        }

        if (_doDebug)
        {
            Debug.Log($"{_role.group.id}.{_role.id}: Beat[{_beatIndex}] t={beatT:F3} time={beatTime:F3}: Play dance action {danceAction.actionName}.{danceAction.variantName}'" +
                $", movement: {danceAction.movement}, duration={danceAction.duration:F3}, beatDuration={beatDuration:F3}");
        }

        if (_danceActionRoutine != null)
        {
            if (_doDebug)
                Debug.LogWarning("Stopping old dance action routine");

            StopCoroutine(_danceActionRoutine);
        }

        _currentDanceAction = danceAction;

        transform.rotation = GetRotationFromDirection(danceAction.startFacing);

        _danceActionRoutine = DanceActionRoutine(danceAction, beatTime, beatT, beatDuration);
        StartCoroutine(_danceActionRoutine);
    }

    private IEnumerator DanceActionRoutine(DanceAction danceAction, float beatTime, float beatT, float beatDuration)
    {
        float timeScale = (danceAction.animationDuration / beatDuration) / danceAction.duration;

        if (_doDebug)
        {
            Debug.Log($"{_role.group.id}.{_role.id}: Beat[{_beatIndex}] t={beatT:F3} time={beatTime:F3}: Starting to play dance action " +
                $"{danceAction.actionName}.{danceAction.variantName}', movement: {danceAction.movement}, animation: '{danceAction.animationClip.name}'\n" +
                $"Action duration={danceAction.duration}, animationDuration={danceAction.animationDuration}, beatDuration={beatDuration:F3}, timeScale={timeScale:F3}");
        }

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
        if (_doDebug)
            Debug.Log($"{_role.group.id}.{_role.id}: Beat[{_beatIndex}]: Finished playing dance action '{danceAction.actionName}.{danceAction.variantName}', movement: '{danceAction.movement}'");

        transform.position = GetPosition();
        transform.rotation = GetRotation();
        _dancer.localPosition = Vector3.zero;
        _dancer.localRotation = Quaternion.identity;

        _isTransitioning = false;
        _isTransitionComplete = false;
        _currentDanceAction = null;
    }

    public Vector3 GetPosition()
    {
        if (_dancer == null || _currentDanceAction == null)
            return transform.position;

        if (_currentDanceAction.movement != null && _currentDanceAction.movement.directions != null && _currentDanceAction.movement.directions.Length > 0)
        {
            DanceVector vector = _currentDanceAction.movement.directions[0]; // TODO: Only first direction taken here, should take all as a compound and normalize it
            return transform.TransformPoint(OrientateVector(_dancer.localPosition, vector));
        }

        return transform.TransformPoint(_dancer.localPosition);
    }

    public Quaternion GetRotation()
    {
        return Quaternion.FromToRotation(Vector3.up, GetDirection());
    }

    public Vector3 GetDirection()
    {
        if (_currentDanceAction != null && _currentDanceAction.transition != null && (_isTransitioning || _isTransitionComplete))
        {
            if (_currentDanceAction.transition.direction == DanceDirection.CW)
                return Quaternion.AngleAxis(-_transitionAmount, Vector3.forward) * _dancer.up;

            if (_currentDanceAction.transition.direction == DanceDirection.CCW)
                return Quaternion.AngleAxis(_transitionAmount, Vector3.forward) * _dancer.up;
        }

        return _dancer.up;
    }

    public Vector3 OrientateVector(Vector3 localVector, DanceVector danceVector)
    {
        return GetRotationFromDirection(danceVector.direction) * localVector * danceVector.distance;
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

    public Quaternion GetRotationFromDirection(DanceDirection danceDirection)
    {
        switch (danceDirection)
        {
            case DanceDirection.Down: return Quaternion.AngleAxis(180, Vector3.forward);
            case DanceDirection.Left: return Quaternion.AngleAxis(90, Vector3.forward);
            case DanceDirection.Right: return Quaternion.AngleAxis(-90, Vector3.forward);
            default: break;
        }

        return Quaternion.identity;
    }
}

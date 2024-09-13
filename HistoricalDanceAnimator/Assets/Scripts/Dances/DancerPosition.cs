using System.Collections;
using UnityEngine;

public class DancerPosition : MonoBehaviour
{
    public bool _doDebug;
    public bool _doDebugTransitions;

    /// <summary>
    /// Transform moved by dance action animations,
    /// used to calculate dancer pawn position.
    /// </summary>
    public Transform _dancer;

    /// <summary>
    /// Transform moved by dance action animations,
    /// used to offset dancer position or rotation point during animations
    /// </summary>
    public Transform _pivot;

    /// <summary>
    /// Animation component to run dance animations animations on.
    /// </summary>
    public Animation _animation;

    private DanceFormation _formation;
    private DancerRole _role;
    private Pawn _pawn;
    private int _dancerIndex;
    private int _setPositionIndex;

    private GameObject _debugDancerPosition;

    private int _beatIndex;
    private float _beatT;
    private float _beatTime;
    private float _danceTime;

    private float _actionStartTime;
    private float _actionCurrentTime;
    private float _actionEndTime;
    private float _actionDuration;
    private float _actionT;
    private float _actionTimeScale;

    [HideInInspector] private Vector3 _dancerLocalPosition;
    [HideInInspector] private Vector3 _dancerWorldPosition;
    [HideInInspector] private Vector3 _lookDirection;
    [HideInInspector] private Quaternion _lookRotation;
    [HideInInspector] private DanceAction _currentDanceAction;
    [HideInInspector] private DanceAction _nextDanceAction;

    public DancerRole Role { get { return _role; } }
    public Pawn Pawn { get { return _pawn; } }
    public int DancerIndex { get { return _dancerIndex; } }
    public int SetPositionIndex { get { return _setPositionIndex; } set { _setPositionIndex = value; } }
    public bool IsDancing { get { return _currentDanceAction != null; } }

    public void Init(DanceFormation formation, int dancerIndex, int setPositionIndex, GameObject debugDancerPositionPrefab)
    {
        _formation = formation;
        _dancerIndex = dancerIndex;
        _setPositionIndex = setPositionIndex;

        _pivot = new GameObject("Pivot").transform;
        _pivot.parent = transform;
        _pivot.localPosition = Vector3.zero;
        _pivot.localRotation = Quaternion.identity;

        _dancer = new GameObject("Dancer").transform;
        _dancer.parent = _pivot;
        _dancer.localPosition = Vector3.zero;
        _dancer.localRotation = Quaternion.identity;

        _debugDancerPosition = Instantiate(debugDancerPositionPrefab, _dancer);

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

    public void SetPawn(Pawn pawn)
    {
        _pawn = pawn;
    }

    public void OnDanceBegun()
    {
        _beatIndex = -1;
        OnDanceRepeat();
    }

    public void OnDanceRepeat()
    {
        _role.ResetActionTransitions();
    }

    public void DanceUpdate(float danceTime, int beatIndex, float beatTime, float beatT, float beatDuration)
    {
        _beatT = beatT;
        _beatTime = beatTime;
        _danceTime = danceTime;

        if (_currentDanceAction != null)
        {
            UpdateDanceAction(_currentDanceAction, beatTime, beatT, beatDuration);
        }

        if (beatIndex != _beatIndex)
        {
            _beatIndex = beatIndex;

            if (_role.TryGetAction(_beatIndex, out _nextDanceAction))
            {
                if (_currentDanceAction != null && _currentDanceAction != _nextDanceAction && _actionT < 1f)
                {
                    if (_doDebug)
                    {
                        float remainingT = 1f - _actionT;
                        float remainingTime = _actionEndTime - _actionCurrentTime;
                        Debug.Log($"F({Time.frameCount}): {_role.group.id}.{_role.id}: Beat[{_beatIndex}] t={beatT:F3} time={beatTime:F3}: Interrupting current dance action " +
                                  $"'{_currentDanceAction.actionName}' at t={_actionT:F3} time={_actionCurrentTime:F3}s, remainingT={remainingT:F3} remainingTime={remainingTime:F3}s");
                    }

                    OnDanceActionCompleted(_currentDanceAction);
                }

                BeginDanceActionRoutine(_nextDanceAction, beatTime, beatT, beatDuration);
            }
            //else
            //{
            //    Debug.LogError($"{name}: Unable to get DanceAction at beat {_beatIndex}");
            //}
        }

        if (_currentDanceAction == null)
            return;

        UpdateDanceAction(_currentDanceAction, beatTime, beatT, beatDuration);

        if (_currentDanceAction.transitions != null && _currentDanceAction.transitions.HasTransitions())
        {
            if (_currentDanceAction.transitions.IsTransitioningPosition() && _doDebugTransitions)
                Debug.Log($"F({Time.frameCount}): {_role.group.id}.{_role.id}: Beat[{_beatIndex}]: Applying position transition of offset={_currentDanceAction.transitions.PositionOffset} at t={_actionT}");
            if (_currentDanceAction.transitions.IsTransitioningRotation() && _doDebugTransitions)
                Debug.Log($"F({Time.frameCount}): {_role.group.id}.{_role.id}: Beat[{_beatIndex}]: Applying rotation transition of angle={_currentDanceAction.transitions.RotationOffset} at t={_actionT}");

            _currentDanceAction.transitions.OnDanceUpdate(_actionT, _doDebug);
        }
    }

    private void UpdateActionTime()
    {
        _actionCurrentTime = _danceTime - _actionStartTime;
        _actionT = _actionCurrentTime / _actionDuration;
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
            Debug.Log($"F({Time.frameCount}): {_role.group.id}.{_role.id}: Beat[{_beatIndex}] t={beatT:F3} time={beatTime:F3}: Play dance action {danceAction.actionName}.{danceAction.variantName}'" +
                $", movement: {danceAction.movement}, duration={danceAction.duration:F3}, beatDuration={beatDuration:F3}");
        }

        BeginDanceActionRoutine(danceAction, beatTime, beatT, beatDuration);
    }

    private void BeginDanceActionRoutine(DanceAction danceAction, float beatTime, float beatT, float beatDuration)
    {
        _currentDanceAction = danceAction;
        _actionStartTime = _danceTime - beatTime;
        _actionEndTime = _actionStartTime + danceAction.duration * beatDuration;
        _actionDuration = _actionEndTime - _actionStartTime;
        _actionTimeScale = danceAction.animationDuration / (beatDuration * danceAction.duration);

        if (_doDebug)
        {
            Debug.Log($"F({Time.frameCount}): {_role.group.id}.{_role.id}: Beat[{_beatIndex}] t={beatT:F3} time={beatTime:F3}: Starting to play dance action {danceAction.actionName}.{danceAction.variantName}',\n" +
                $"movement: {danceAction.movement}, facing: {danceAction.startFacing}, animation: '{danceAction.animationClip.name}', animationDuration={danceAction.animationDuration}\n" +
                $"Action startTime={_actionStartTime:F3}, endTime={_actionEndTime:F3}, duration={_actionDuration:F3}, " +
                $"beatLength={danceAction.duration}, beatDuration={beatDuration:F3}, timeScale={_actionTimeScale:F3}");
        }

        if (!_animation.IsPlaying(danceAction.animationClip.name))
            _animation.Play(danceAction.animationClip.name);

        // Set animation at correct time since we almost never really start at t=0
        _animation[danceAction.animationClip.name].time = beatTime;

        foreach (AnimationState state in _animation)
            state.speed = _actionTimeScale;

        transform.rotation = DanceUtility.GetRotationFromDirection(danceAction.startFacing);
        if (_doDebug)
        {
            Debug.LogWarning($"Rotation from {danceAction.startFacing} resulted in euler angles: {transform.rotation.eulerAngles}");
        }
    }

    private void UpdateDanceAction(DanceAction danceAction, float beatTime, float beatT, float beatDuration)
    {
        UpdateActionTime();

        if (_actionT >= 1f || !_animation.isPlaying)
            OnDanceActionCompleted(danceAction);
    }

    public void EndDanceAction()
    {
        if (_currentDanceAction != null)
        {
            _actionT = 1f;
            OnDanceActionCompleted(_currentDanceAction);
        }
        else
            Debug.LogWarning($"{_role.group.id}.{_role.id}: Beat[{_beatIndex}]: No dance action to end!");
    }

    private void OnDanceActionCompleted(DanceAction danceAction)
    {
        if (_doDebug)
            Debug.Log($"F({Time.frameCount}): {_role.group.id}.{_role.id}: Beat[{_beatIndex}]: Finished playing dance action '{danceAction.actionName}.{danceAction.variantName}', movement: '{danceAction.movement}'");

        if (danceAction.transitions != null)
            danceAction.transitions.OnDanceUpdate(1f, _doDebug);

        transform.position = GetPosition();
        transform.rotation = GetRotation();

        if (_doDebug)
            Debug.Log($"F({Time.frameCount}): {_role.group.id}.{_role.id}: Beat[{_beatIndex}]: Action ended, position={transform.position}, rotation={transform.rotation.eulerAngles}");

        _dancerLocalPosition = Vector3.zero;

        _pivot.localPosition = Vector3.zero;
        _pivot.localRotation = Quaternion.identity;
        _pivot.localScale = Vector3.one;

        _dancer.localPosition = Vector3.zero;
        _dancer.localRotation = Quaternion.identity;
        _dancer.localScale = Vector3.one;

        //_isTransitioning = false;
        //_isTransitionComplete = false;

        if (danceAction == _currentDanceAction)
            _currentDanceAction = null;
    }

    public Vector3 GetPosition()
    {
        if (_dancer == null || _currentDanceAction == null)
            return transform.position;

        _dancerLocalPosition = transform.InverseTransformPoint(_dancer.position);

        if (_currentDanceAction.movement != null && _currentDanceAction.movement.directions != null && _currentDanceAction.movement.directions.Length > 0)
        {
            _dancerLocalPosition.x *= _currentDanceAction.movement.cross.magnitude;
            _dancerLocalPosition.y *= _currentDanceAction.movement.vector.magnitude;
        }

        if (_currentDanceAction.transitions != null && _currentDanceAction.transitions.HasPositionTransitions())
            _dancerLocalPosition += _currentDanceAction.transitions.PositionOffset;

        return transform.TransformPoint(_dancerLocalPosition);
    }

    public Quaternion GetRotation()
    {
        if (_dancer == null || _currentDanceAction == null)
            return transform.rotation;

        _lookDirection = GetDirection();

        _lookRotation = Quaternion.FromToRotation(Vector3.up, _lookDirection);
        if (_lookRotation.x == 1f)
        {
            // Crude hack to fix pawns blinking when look direction is exactly Vector3.down (180 degree offset from Vector3.up)
            //Debug.LogWarning($"{_role.group.id}.{_role.id}: Beat[{_beatIndex}]: Erroneus rotation detected: {_lookRotation.eulerAngles}");
            _lookRotation = Quaternion.AngleAxis(180f, _formation.transform.forward);
        }

        return _lookRotation;
    }

    public Vector3 GetDirection()
    {
        if (_currentDanceAction != null && _currentDanceAction.transitions != null && _currentDanceAction.transitions.HasRotationTransitions())
            return Quaternion.AngleAxis(_currentDanceAction.transitions.RotationOffset, Vector3.forward) * _dancer.up;

        return _dancer.up;
    }

    public Vector3 GetScale()
    {
        return _dancer.localScale;
    }

    public void OnResume()
    {
        foreach (AnimationState state in _animation)
            state.speed = _actionTimeScale;
    }

    public void OnPause()
    {
        foreach (AnimationState state in _animation)
            state.speed = 0f;
    }
}

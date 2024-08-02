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

    //private bool _isTransitioning;
    //private bool _isTransitionComplete;
    //private int _transitionIndex;
    //private float _transitionT;
    //private float _transitionAmount;

    [HideInInspector] private Vector3 _lookDirection;
    [HideInInspector] private Quaternion _lookRotation;
    [HideInInspector] private DanceAction _currentDanceAction;
    [HideInInspector] private DanceAction _nextDanceAction;

    private IEnumerator _danceActionRoutine;

    public DancerRole Role { get { return _role; } }

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

        if (_currentDanceAction == null)
            return;
        
        _actionTime = _danceTime - _currentDanceAction.time * beatDuration;
        _actionT = _actionTime / (_currentDanceAction.duration * beatDuration);

        if (_currentDanceAction.transitions != null && _currentDanceAction.transitions.HasTransitions())
        {
            _currentDanceAction.transitions.OnDanceUpdate(_actionT);
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
        //_transitionIndex = -1;

        transform.rotation = DanceUtility.GetRotationFromDirection(danceAction.startFacing);

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

        //_isTransitioning = false;
        //_isTransitionComplete = false;
        _currentDanceAction = null;
    }

    public Vector3 GetPosition()
    {
        if (_dancer == null || _currentDanceAction == null)
            return transform.position;

        if (_currentDanceAction.movement != null && _currentDanceAction.movement.directions != null && _currentDanceAction.movement.directions.Length > 0)
            return transform.TransformPoint(
                _currentDanceAction.movement.cross * _dancer.localPosition.x +
                _currentDanceAction.movement.vector * _dancer.localPosition.y
            );

        if (_currentDanceAction.transitions != null && _currentDanceAction.transitions.HasPositionTransitions())
            return transform.TransformPoint(_currentDanceAction.transitions.PositionOffset + _dancer.localPosition);

        return transform.TransformPoint(_dancer.localPosition);
    }

    public Quaternion GetRotation()
    {
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
        {
            return Quaternion.AngleAxis(_currentDanceAction.transitions.RotationOffset, Vector3.forward) * _dancer.up;
        }

        return _dancer.up;
    }
}

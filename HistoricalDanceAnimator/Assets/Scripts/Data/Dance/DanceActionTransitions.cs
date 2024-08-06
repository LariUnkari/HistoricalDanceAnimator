using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DanceActionTransitions
{
    public List<DanceActionPositionTransition> positions = new List<DanceActionPositionTransition>();
    public List<DanceActionRotationTransition> rotations = new List<DanceActionRotationTransition>();

    private bool hasPositionTransitions = false;
    private bool isTransitioningPosition = false;
    private bool hasRotationTransitions = false;
    private bool isTransitioningRotation = false;

    private int positionTransitionIndex;
    private float positionTransitionT;
    private Vector3 positionOffset;
    private Vector3 positionFrom;
    private Vector3 positionTo;

    private int rotationTransitionIndex;
    private float rotationTransitionT;
    private float rotationOffset;
    private float rotationFrom;
    private float rotationTo;

    public Vector3 PositionOffset { get { return positionOffset; } }
    public float RotationOffset { get { return rotationOffset; } }

    public bool HasTransitions()
    {
        return hasPositionTransitions || hasRotationTransitions;
    }

    public bool HasPositionTransitions()
    {
        return hasPositionTransitions;
    }

    public bool HasRotationTransitions()
    {
        return hasRotationTransitions;
    }

    public void AddTransition(DanceActionPositionTransition transition)
    {
        hasPositionTransitions = true;
        positions.Add(transition);
    }

    public void Reset()
    {
        positionTransitionIndex = -1;
        positionTransitionT = 0f;
        positionOffset = Vector3.zero;
        positionFrom = Vector3.zero;
        positionTo = Vector3.zero;

        rotationTransitionIndex = -1;
        rotationTransitionT = 0f;
        rotationOffset = 0f;
        rotationFrom = 0f;
        rotationTo = 0f;
    }

    public void AddTransition(DanceActionRotationTransition transition)
    {
        hasRotationTransitions = true;
        rotations.Add(transition);
    }

    public void OnDanceUpdate(float actionT, bool doDebug)
    {
        CheckPositionTransitions(actionT, doDebug);
        CheckRotationTransitions(actionT, doDebug);
    }

    private void CheckPositionTransitions(float actionT, bool doDebug)
    {
        DanceActionPositionTransition transition;

        for (int i = Mathf.Max(0, positionTransitionIndex); i < positions.Count; i++)
        {
            transition = positions[i];

            if (isTransitioningPosition && i == positionTransitionIndex && actionT >= transition.endTime)
            {
                // Transition ended
                if (doDebug)
                    Debug.Log($"PositionTransition[{i}] ended at t:{actionT:F3}>={transition.endTime:F3}, vector={transition.vector}");

                positionTransitionT = 1f;
                positionOffset = positionTo;
                isTransitioningPosition = false;
            }

            if (!isTransitioningPosition && i > positionTransitionIndex && actionT >= transition.time)
            {
                // New transition starting
                if (doDebug)
                    Debug.Log($"PositionTransition[{i}] starting at t:{actionT:F3}>={transition.time:F3}, vector={transition.vector}");

                isTransitioningPosition = true;
                positionTransitionIndex = i;
                positionFrom = positionOffset;
                positionTo = positionFrom + transition.vector;

                // Catch transition ending before starting
                if (actionT >= transition.endTime)
                    i--;
            }

            if (isTransitioningPosition && i == positionTransitionIndex)
            {
                positionTransitionT = (actionT - transition.time) / transition.duration;
                positionOffset = Vector3.Lerp(positionFrom, positionTo, positionTransitionT);
            }
        }
    }

    private void CheckRotationTransitions(float actionT, bool doDebug)
    {
        DanceActionRotationTransition transition;

        for (int i = Mathf.Max(0, rotationTransitionIndex); i < rotations.Count; i++)
        {
            transition = rotations[i];

            if (isTransitioningRotation && i == rotationTransitionIndex && actionT >= transition.endTime)
            {
                // Transition ended
                if (doDebug)
                    Debug.Log($"RotationTransition[{i}] ending at t:{actionT:F3}>={transition.endTime:F3}, direction={transition.direction}, amount={transition.amount}");

                rotationTransitionT = 1f;
                rotationOffset = rotationTo;
                isTransitioningRotation = false;
            }

            if (!isTransitioningRotation && i > rotationTransitionIndex && actionT >= transition.time)
            {
                // New transition starting
                if (doDebug)
                    Debug.Log($"RotationTransition[{i}] starting at t:{actionT:F3}>={transition.time:F3}, direction={transition.direction}, amount={transition.amount}");

                isTransitioningRotation = true;
                rotationTransitionIndex = i;
                rotationFrom = rotationOffset;
                rotationTo = rotationFrom + (transition.direction == DanceDirection.CW ? -transition.amount : transition.amount);

                // Catch transition ending before starting
                if (actionT >= transition.endTime)
                    i--;
            }

            if (isTransitioningRotation && i == rotationTransitionIndex)
            {
                rotationTransitionT = (actionT - transition.time) / transition.duration;
                rotationOffset = Mathf.Lerp(rotationFrom, rotationTo, rotationTransitionT);
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DanceActionTransitions
{
    public List<DanceActionPositionTransition> positions = new List<DanceActionPositionTransition>();
    public List<DanceActionRotationTransition> rotations = new List<DanceActionRotationTransition>();

    private bool hasPositionTransitions = false;
    private bool isTransitioningPosition = false;
    private int positionTransitionIndex = -1;
    private float positionTransitionT = 0f;
    private Vector3 positionOffset = Vector3.zero;
    private Vector3 positionFrom = Vector3.zero;
    private Vector3 positionTo = Vector3.zero;

    private bool hasRotationTransitions = false;
    private bool isTransitioningRotation = false;
    private int rotationTransitionIndex = -1;
    private float rotationTransitionT = 0f;
    private float rotationOffset = 0f;
    private float rotationFrom = 0f;
    private float rotationTo = 0f;

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

    public void AddTransition(DanceActionRotationTransition transition)
    {
        hasRotationTransitions = true;
        rotations.Add(transition);
    }

    public void OnDanceUpdate(float actionT)
    {
        CheckPositionTransitions(actionT);
        CheckRotationTransitions(actionT);
    }

    private void CheckPositionTransitions(float actionT)
    {
        DanceActionPositionTransition transition;

        for (int i = Mathf.Max(0, positionTransitionIndex); i < positions.Count; i++)
        {
            transition = positions[i];

            if (isTransitioningPosition && i == positionTransitionIndex && actionT >= transition.endTime)
            {
                // Transition ended
                positionTransitionT = 1f;
                positionOffset = positionTo;
                isTransitioningPosition = false;
            }

            if (!isTransitioningPosition && i > positionTransitionIndex && actionT >= transition.time)
            {
                // New transition starting
                isTransitioningPosition = true;
                positionTransitionIndex = i;
                positionFrom = positionOffset;
                positionTo = positionFrom + transition.vector;
            }

            if (isTransitioningPosition && i == positionTransitionIndex)
            {
                positionTransitionT = (actionT - transition.time) / transition.duration;
                positionOffset = Vector3.Lerp(positionFrom, positionTo, positionTransitionT);
                break;
            }
        }
    }

    private void CheckRotationTransitions(float actionT)
    {
        DanceActionRotationTransition transition;

        for (int i = Mathf.Max(0, rotationTransitionIndex); i < rotations.Count; i++)
        {
            transition = rotations[i];

            if (isTransitioningRotation && i == rotationTransitionIndex && actionT >= transition.endTime)
            {
                // Transition ended
                rotationTransitionT = 1f;
                rotationOffset = rotationTo;
                isTransitioningRotation = false;
            }

            if (!isTransitioningRotation && i > rotationTransitionIndex && actionT >= transition.time)
            {
                // New transition starting
                isTransitioningRotation = true;
                rotationTransitionIndex = i;
                rotationFrom = rotationOffset;
                rotationTo = rotationFrom + (transition.direction == DanceDirection.CW ? -transition.amount : transition.amount);
            }

            if (isTransitioningRotation && i == rotationTransitionIndex)
            {
                rotationTransitionT = (actionT - transition.time) / transition.duration;
                rotationOffset = Mathf.Lerp(rotationFrom, rotationTo, rotationTransitionT);
                break;
            }
        }
    }
}

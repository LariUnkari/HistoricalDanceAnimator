using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DanceActionTransitions
{
    public List<DanceActionTransition> positions = new List<DanceActionTransition>();
    public List<DanceActionTransition> rotations = new List<DanceActionTransition>();

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

    public void AddTransition(DanceActionTransition transition)
    {
        if (transition.direction == DanceDirection.CW || transition.direction == DanceDirection.CCW)
        {
            hasRotationTransitions = true;
            rotations.Add(transition);
        }
        else
        {
            hasPositionTransitions = true;
            positions.Add(transition);
        }
    }

    public void OnDanceUpdate(float actionT)
    {
        DanceActionTransition transition;
        int i;

        for (i = Mathf.Max(0, positionTransitionIndex); i < positions.Count; i++)
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
                positionTo = positionFrom;

                switch (transition.direction)
                {
                    case DanceDirection.Up:
                        positionTo += Vector3.up;
                        break;
                    case DanceDirection.Down:
                        positionTo += Vector3.down;
                        break;
                    case DanceDirection.Left:
                        positionTo += Vector3.left;
                        break;
                    case DanceDirection.Right:
                        positionTo += Vector3.right;
                        break;
                }
            }

            if (isTransitioningPosition && i == positionTransitionIndex)
            {
                positionTransitionT = (actionT - transition.time) / transition.duration;
                positionOffset = Vector3.Lerp(positionFrom, positionTo, positionTransitionT);
                break;
            }
        }

        for (i = Mathf.Max(0, rotationTransitionIndex); i < rotations.Count; i++)
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

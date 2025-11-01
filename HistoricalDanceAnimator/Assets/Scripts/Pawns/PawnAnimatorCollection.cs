using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnAnimatorCollection : MonoBehaviour
{
    public PawnAnimator[] pawnAnimators;

    public void SetVisual(int setIndex)
    {
        foreach (PawnAnimator pawnAnimator in pawnAnimators)
            pawnAnimator.SetVisual(setIndex);
    }
}

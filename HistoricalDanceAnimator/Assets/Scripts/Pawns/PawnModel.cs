using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PawnModel : MonoBehaviour
{
    public MeshRenderer _background;
    public MeshRenderer _foreground;
    public TextMeshPro _label;

    public void SetText(string text)
    {
        _label.text = text;
    }
}

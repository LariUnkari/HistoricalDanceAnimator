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

    public void SetVisualsFromPreset(PawnModelPreset preset)
    {
        _label.color = preset.labelColor;
        _foreground.material.color = preset.foregroundColor;
        _background.material.color = preset.backgroundColor;
    }
}

using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PawnAnimator : MonoBehaviour
{
    [System.Serializable]
    public class VisualSet
    {
        public List<Material> bgMats;
        public List<Material> fgMats;
        public string labelText;
        public Color labelColor = Color.white;
    }

    public MeshRenderer background;
    public MeshRenderer foreground;
    public TextMeshPro label;

    public VisualSet[] visualSets;

    public void SetVisual(int setIndex)
    {
        if (setIndex < 0 || setIndex >= visualSets.Length)
            return;

        VisualSet set = visualSets[setIndex];

        if (background != null) background.SetMaterials(set.bgMats);
        if (foreground != null) foreground.SetMaterials(set.fgMats);
        if (label != null)
        {
            label.SetText(set.labelText);
            label.color = set.labelColor;
        }
    }
}

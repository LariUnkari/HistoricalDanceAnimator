using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New PawnModelPreset", menuName = "ScriptableObject/Pawn/PawnModelPreset")]
public class PawnModelPreset : ScriptableObject
{
    public string role = "Lord";
    public string group = "A";
    public string variant = "1";
    public Color foregroundColor = Color.grey;
    public Color backgroundColor = Color.white;
    public Color labelColor = Color.black;
    public GameObject model;
}

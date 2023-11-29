using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New PawnModelPreset", menuName = "ScriptableObject/Pawn/PawnModelPreset")]
public class PawnModelPreset : ScriptableObject
{
    public string role = "Lord";
    public string group = "A";
    public string variant = "1";
    public GameObject model;
}

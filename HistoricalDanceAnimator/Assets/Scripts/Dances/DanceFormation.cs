using System.Collections.Generic;
using UnityEngine;

public class DanceFormation : MonoBehaviour
{
    public Transform _pawnParent;
    public Pawn[] _dancerPawns;

    public Transform _positionParent;
    public DancerPosition[] _dancerPositions;

    private DancePart _currentPart;

    private Dictionary<int, DancePart> _dancePartsOnBeat;
    private Dictionary<string, List<DancerPosition>> _dancersByRole;

    private int _beatIndex;

    public void SetFormation(DanceData danceData)
    {
        _pawnParent = new GameObject("Pawns").transform;
        _pawnParent.parent = transform;
        _positionParent = new GameObject("Positions").transform;
        _positionParent.parent = transform;

        _dancePartsOnBeat = new Dictionary<int, DancePart>();

        DancePart part;
        for (int i = 0; i < danceData.parts.Length; i++)
        {
            part = danceData.parts[i];
            _dancePartsOnBeat.Add(part.time, part);
        }

        _dancersByRole = new Dictionary<string, List<DancerPosition>>();

        _dancerPawns = new Pawn[danceData.placements.Length];
        // TODO: Add extra positions if dancers wait to change role etc.
        _dancerPositions = new DancerPosition[danceData.placements.Length];

        string roleKey;
        DancerRole dancerRole;
        DancerPosition position;
        DancerPlacement placement;
        List<DancerPosition> rolePositions;

        for (int i = 0; i < danceData.placements.Length; i++)
        {
            placement = danceData.placements[i];
            position = CreatePosition(placement);
            _dancerPositions[i] = position;

            roleKey = DancerRole.GetRoleKey(placement.group, placement.role);

            if (danceData.TryGetRole(roleKey, out dancerRole))
                position.SetRole(dancerRole);

            if (!_dancersByRole.TryGetValue(roleKey, out rolePositions))
            {
                rolePositions = new List<DancerPosition>();
                _dancersByRole.Add(roleKey, rolePositions);
            }

            rolePositions.Add(position);

            _dancerPawns[i] = CreateDancer(placement, position);
        }
    }

    private DancerPosition CreatePosition(DancerPlacement placement)
    {
        string name = $"Position_{placement.group}-{placement.role}";
        Debug.Log($"Creating position {name}");

        GameObject go = new GameObject(name);
        go.transform.parent = _positionParent;
        go.transform.localPosition = placement.position;

        DancerPosition position = go.AddComponent<DancerPosition>();
        position.Init(this);

        return position;
    }

    private Pawn CreateDancer(DancerPlacement placement, DancerPosition dancerPosition)
    {
        string name = $"Dancer_{placement.group}-{placement.role}-{placement.variant}";
        Debug.Log($"Creating dancer {name}");

        GameObject dancer = new GameObject(name);
        Pawn pawn = dancer.AddComponent<Pawn>();
        pawn.SetDancerPosition(dancerPosition);

        PawnModelPreset preset = GetPawnModelPreset(placement);
        GameObject model = Instantiate(preset.model);
        model.transform.parent = dancer.transform;

        PawnModel pawnModel = model.GetComponent<PawnModel>();
        pawn.model = pawnModel;
        pawnModel._label.text = placement.group;
        pawnModel._label.color = preset.labelColor;
        pawnModel._foreground.material.color = preset.foregroundColor;
        pawnModel._background.material.color = preset.backgroundColor;

        dancer.transform.parent = _pawnParent;
        dancer.transform.localPosition = placement.position;

        return pawn;
    }

    private PawnModelPreset GetPawnModelPreset(DancerPlacement dancerPosition)
    {
        return PawnModelDatabase.GetInstance().GetPreset(PawnModelDatabase.GetPresetKey(dancerPosition.role, "", dancerPosition.variant));
    }

    public void BeginDance()
    {
        _beatIndex = -1;

        foreach (DancerPosition dancerPosition in _dancerPositions)
            dancerPosition.BeginDance();
    }

    public void DanceUpdate(float danceTime, int beatIndex, float beatTime, float beatT, float beatDuration)
    {
        if (beatIndex > _beatIndex)
        {
            //Debug.Break();
            _beatIndex = beatIndex;

            DancePart part;
            if (_dancePartsOnBeat.TryGetValue(_beatIndex, out part))
            {
                _currentPart = part;
                Debug.Log($"Dance progressed to beat {beatIndex} at time {danceTime:F3}, moving to part {_currentPart.name}. Current beat t={beatT:F3}, time={beatTime:F3}, duration={beatDuration:F3}");
            }
            else
            {
                Debug.Log($"Dance progressed to beat {beatIndex} at time {danceTime:F3} in part {_currentPart.name}. Current beat t={beatT:F3}, time={beatTime:F3}, duration={beatDuration:F3}");
            }
        }

        foreach (DancerPosition dancerPosition in _dancerPositions)
            dancerPosition.DanceUpdate(danceTime, _beatIndex, beatTime, beatT, beatDuration);
    }
}

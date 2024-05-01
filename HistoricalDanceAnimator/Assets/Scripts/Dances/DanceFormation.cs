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
        GameObject go = new GameObject($"Position_{placement.group}-{placement.role}");
        go.transform.parent = _positionParent;
        go.transform.localPosition = placement.position;

        DancerPosition position = go.AddComponent<DancerPosition>();
        position.Init();

        return position;
    }

    private Pawn CreateDancer(DancerPlacement placement, DancerPosition dancerPosition)
    {
        GameObject dancer = new GameObject($"Dancer_{placement.role}{placement.group}");
        Pawn pawn = dancer.AddComponent<Pawn>();
        pawn._dancerPosition = dancerPosition;

        GameObject model = Instantiate(GetPawnModelPreset(placement).model);
        model.transform.parent = dancer.transform;

        dancer.transform.parent = _pawnParent;
        dancer.transform.localPosition = placement.position;

        return pawn;
    }

    private PawnModelPreset GetPawnModelPreset(DancerPlacement dancerPosition)
    {
        return PawnModelDatabase.GetInstance().GetPreset(PawnModelDatabase.GetPresetKey(dancerPosition.role, dancerPosition.group, dancerPosition.variant));
    }

    public void BeginDance()
    {
        _beatIndex = -1;

        foreach (DancerPosition dancerPosition in _dancerPositions)
            dancerPosition.BeginDance();
    }

    public void DanceUpdate(float danceTime, float beatTime, float beatDuration, int beatIndex)
    {
        if (beatIndex > _beatIndex)
        {
            //Debug.Break();
            _beatIndex = beatIndex;

            DancePart part;
            if (_dancePartsOnBeat.TryGetValue(_beatIndex, out part))
            {
                _currentPart = part;
                Debug.Log($"Dance progressed to beat {beatIndex} at time {danceTime:F3}, moving to part {_currentPart.name}. Current beat time {beatTime:F3}, beat duration {beatDuration:F3}");
            }
            else
            {
                Debug.Log($"Dance progressed to beat {beatIndex} at time {danceTime:F3} in part {_currentPart.name}. Current beat time {beatTime:F3}, beat duration {beatDuration:F3}");
            }
        }

        foreach (DancerPosition dancerPosition in _dancerPositions)
            dancerPosition.DanceUpdate(danceTime, beatTime, beatDuration, _beatIndex);
    }
}

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
    private int _repeatIndex;
    public string CurrentPart { get { return _currentPart != null ? _currentPart.name : ""; } }

    public void SetFormation(DanceData danceData, GameObject debugDancerPositionPrefab)
    {
        _pawnParent = new GameObject("Pawns").transform;
        _pawnParent.parent = transform;
        _positionParent = new GameObject("Positions").transform;
        _positionParent.parent = transform;

        _dancePartsOnBeat = new Dictionary<int, DancePart>();

        DancePart part;
        for (int i = 0; i < danceData.choreography.Length; i++)
        {
            part = danceData.choreography[i];
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
        Vector2 minorSetOffset = Vector2.zero;

        if (danceData.danceSet.sizeCount > 1)
            Debug.Log($"Creating a formation of {danceData.danceSet.sizeCount} minor sets of {danceData.placements.Length} placements");
        else
            Debug.Log($"Creating a formation of {danceData.placements.Length} placements");

        for (int i = 0; i < danceData.danceSet.sizeCount; i++)
        {
            // Calculate minor set offset if applicable
            if (danceData.danceSet.sizeCount > 1)
                minorSetOffset = Vector2.down * (i - (danceData.danceSet.sizeCount - 1) / 2f) * danceData.danceSet.separation;

            for (int k = 0; k < danceData.placements.Length; k++)
            {
                placement = danceData.placements[k];
                position = CreatePosition(placement, i, minorSetOffset, debugDancerPositionPrefab);
                _dancerPositions[k] = position;

                roleKey = DancerRole.GetRoleKey(placement.group, placement.role);

                if (danceData.TryGetRole(roleKey, out dancerRole))
                    position.SetRole(dancerRole);

                if (!_dancersByRole.TryGetValue(roleKey, out rolePositions))
                {
                    rolePositions = new List<DancerPosition>();
                    _dancersByRole.Add(roleKey, rolePositions);
                }

                rolePositions.Add(position);

                _dancerPawns[k] = CreateDancer(placement, position);
            }
        }
    }

    private DancerPosition CreatePosition(DancerPlacement placement, int minorSetIndex, Vector2 offset, GameObject debugDancerPositionPrefab)
    {
        string name = $"Position_{placement.group}-{placement.role}";
        Debug.Log($"Creating position {name}");

        GameObject go = new GameObject(name);
        go.transform.parent = _positionParent;
        go.transform.localPosition = placement.position + offset;
        go.transform.rotation = DanceUtility.GetRotationFromDirection(placement.startFacing);

        DancerPosition position = go.AddComponent<DancerPosition>();
        position.Init(this, minorSetIndex, debugDancerPositionPrefab);

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
            dancerPosition.OnDanceBegun();
    }

    private void OnDanceRepeat(int repeatIndex)
    {
        Debug.LogWarning($"Dance repeating for the {DanceUtility.GetOrdinalNumberString(repeatIndex)} time!");
        _repeatIndex = repeatIndex;
        _beatIndex = -1;
    }

    public void DanceUpdate(float danceTime, int beatIndex, int repeatIndex, float beatTime, float beatT, float beatDuration)
    {
        if (repeatIndex > _repeatIndex)
            OnDanceRepeat(repeatIndex);

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
                Debug.Log($"Dance progressed to beat {beatIndex} at time {danceTime:F3} in part {(_currentPart != null ? _currentPart.name : "NULL")}. Current beat t={beatT:F3}, time={beatTime:F3}, duration={beatDuration:F3}");
            }
        }

        foreach (DancerPosition dancerPosition in _dancerPositions)
            dancerPosition.DanceUpdate(danceTime, _beatIndex, beatTime, beatT, beatDuration);
    }

    public void EndDance()
    {
        foreach (DancerPosition dancerPosition in _dancerPositions)
        {
            if (dancerPosition.IsDancing)
                dancerPosition.EndDanceAction();
        }
    }

    public void OnResume()
    {
        foreach (DancerPosition dancerPosition in _dancerPositions)
        {
            dancerPosition.OnResume();
        }
    }

    public void OnPause()
    {
        foreach (DancerPosition dancerPosition in _dancerPositions)
        {
            dancerPosition.OnPause();
        }
    }
}

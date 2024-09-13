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

    private int _setLength;
    private int _beatIndex;
    private int _repeatIndex;

    public string CurrentPart { get { return _currentPart != null ? _currentPart.name : ""; } }
    public int SetLength { get { return _setLength; } }

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

        _setLength = CalculateSetLength(danceData);

        _dancersByRole = new Dictionary<string, List<DancerPosition>>();

        if (danceData.danceSet.sizeCount > 1)
        {
            _dancerPawns = new Pawn[danceData.placements.Length * danceData.danceSet.sizeCount];
            _dancerPositions = new DancerPosition[danceData.placements.Length * danceData.danceSet.sizeCount];
            Debug.Log($"Creating a formation of {danceData.danceSet.sizeCount} minor sets of {danceData.placements.Length} placements, total {_dancerPositions.Length} positions");
        }
        else
        {
            _dancerPawns = new Pawn[danceData.placements.Length];
            _dancerPositions = new DancerPosition[danceData.placements.Length];
            Debug.Log($"Creating a formation of {danceData.placements.Length} placements");
        }

        string roleKey;
        DancerRole dancerRole;
        DancerPosition position;
        DancerPlacement placement;
        List<DancerPosition> rolePositions;
        Vector2 minorSetOffset = Vector2.zero;
        int setPositionIndex;

        Pawn pawn;
        int dancerIndex = 0;
        for (int i = 0; i < danceData.danceSet.sizeCount; i++)
        {
            // Calculate minor set offset if applicable
            if (danceData.danceSet.sizeCount > 1)
                minorSetOffset = Vector2.down * (i - (danceData.danceSet.sizeCount - 1) / 2f) * danceData.danceSet.separation;

            for (int k = 0; k < danceData.placements.Length; k++)
            {
                dancerIndex = i * danceData.placements.Length + k;
                placement = danceData.placements[k];

                setPositionIndex = CalculateSetPositionIndex(danceData.danceSet, i, placement.group);
                position = CreatePosition(placement, dancerIndex, setPositionIndex, minorSetOffset, debugDancerPositionPrefab);
                _dancerPositions[dancerIndex] = position;

                roleKey = DancerRole.GetRoleKey(placement.group, placement.role);

                if (danceData.TryGetRole(roleKey, out dancerRole))
                    position.SetRole(dancerRole);

                if (!_dancersByRole.TryGetValue(roleKey, out rolePositions))
                {
                    rolePositions = new List<DancerPosition>();
                    _dancersByRole.Add(roleKey, rolePositions);
                }

                rolePositions.Add(position);

                pawn = CreateDancer(placement, dancerRole.Variant);
                _dancerPawns[dancerIndex] = pawn;

                pawn.SetDancerPosition(position);
                position.SetPawn(pawn);
            }
        }
    }

    private int CalculateSetLength(DanceData danceData)
    {
        if (danceData.danceSet.form == DanceSetForm.LineLongways)
        {
            if (danceData.danceSet.minorGroups != null)
                return danceData.danceSet.GetMinorSetLength() * danceData.danceSet.sizeCount;
        }

        return danceData.danceSet.sizeCount;
    }

    private int CalculateSetPositionIndex(DanceSet set, int minorSetIndex, string group)
    {
        if (set.form == DanceSetForm.LineLongways)
            return minorSetIndex * set.GetMinorSetLength() + set.GetGroupMinorIndex(group);

        return minorSetIndex;
    }

    private DancerPosition CreatePosition(DancerPlacement placement, int dancerIndex, int setPositionIndex,
        Vector2 offset, GameObject debugDancerPositionPrefab)
    {
        string name = $"Position_{placement.group}-{placement.role}";
        Debug.Log($"Creating position {name}");

        GameObject go = new GameObject(name);
        go.transform.parent = _positionParent;
        go.transform.localPosition = placement.position + offset;
        go.transform.rotation = DanceUtility.GetRotationFromDirection(placement.startFacing);

        DancerPosition position = go.AddComponent<DancerPosition>();
        position.Init(this, dancerIndex, setPositionIndex, debugDancerPositionPrefab);

        return position;
    }

    private Pawn CreateDancer(DancerPlacement placement, string variant)
    {
        string name = $"Dancer_{placement.group}-{placement.role}-{variant}";
        Debug.Log($"Creating dancer {name}");

        GameObject dancer = new GameObject(name);
        Pawn pawn = dancer.AddComponent<Pawn>();

        string key = PawnModelDatabase.GetPresetKey(placement.role, "", variant);

        PawnModelPreset preset;
        if (!PawnModelDatabase.GetInstance().TryGetPreset(key, out preset))
        {
            Debug.LogError($"Unable to find preset with key {key}");
            return pawn;
        }
            
        GameObject model = Instantiate(preset.model);
        model.transform.parent = dancer.transform;

        PawnModel pawnModel = model.GetComponent<PawnModel>();
        pawn.model = pawnModel;
        pawnModel.SetText(placement.group);
        pawnModel.SetVisualsFromPreset(preset);

        dancer.transform.parent = _pawnParent;
        dancer.transform.localPosition = placement.position;

        return pawn;
    }

    public void OnDanceStarted()
    {
        _beatIndex = -1;

        foreach (DancerPosition dancerPosition in _dancerPositions)
            dancerPosition.OnDanceStarted();
    }

    public void OnDanceRepeat(int repeatIndex, DanceData danceData)
    {
        Debug.LogWarning($"F({Time.frameCount}): Dance repeating for the {DanceUtility.GetOrdinalNumberString(repeatIndex)} time!");

        _beatIndex = -1;

        string group;
        foreach (DancerPosition position in _dancerPositions)
        {
            position.OnDanceStarted();

            if (danceData.danceProgression == DanceProgression.Line_AB)
            {
                group = null;

                if (position.Role.group.id.StartsWith(DancerGroup.INACTIVE_ID))
                {
                    if (position.SetPositionIndex == 0)
                        group = "A";
                    else if (position.SetPositionIndex == _setLength - 1)
                        group = "B";
                }
                else
                {
                    if (position.Role.group.id == "A")
                    {
                        position.SetPositionIndex++;

                        if (position.SetPositionIndex == _setLength - 1)
                            group = $"{DancerGroup.INACTIVE_ID}-{position.Role.group.id}";
                    }
                    else if (position.Role.group.id == "B")
                    {
                        position.SetPositionIndex--;

                        if (position.SetPositionIndex == 0)
                            group = $"{DancerGroup.INACTIVE_ID}-{position.Role.group.id}";
                    }
                }

                if (group != null)
                {
                    string key = DancerRole.GetRoleKey(group, position.Role.id);

                    DancerRole role;
                    if (danceData.TryGetRole(key, out role))
                    {
                        Debug.Log($"F({Time.frameCount}): Switching dancer {position.DancerIndex}/{_dancerPositions.Length} at set position {position.SetPositionIndex}/{_setLength} role {position.Role.key} to role {role.key}");
                        ChangeDancerRole(position, role);
                    }
                    else
                    {
                        Debug.LogError($"F({Time.frameCount}): Error in switching dancer {position.DancerIndex}/{_dancerPositions.Length} at set position {position.SetPositionIndex}/{_setLength} role {position.Role.key} to role {key}!");
                    }
                }
            }
        }
    }

    public void UpdateDanceTime(float danceTime)
    {
        foreach (DancerPosition dancerPosition in _dancerPositions)
            dancerPosition.CheckDanceActionEnded(danceTime);
    }

    public void DanceUpdate(float danceTime, int beatIndex, float beatTime, float beatDuration)
    {
        if (beatIndex > _beatIndex)
        {
            // TODO: Improve part logic to not require hitting a specific beat
            DancePart part;
            if (_dancePartsOnBeat.TryGetValue(beatIndex, out part))
            {
                _currentPart = part;
                Debug.Log($"F({Time.frameCount}): Dance progressed to part {_currentPart.name}");
            }
        }

        foreach (DancerPosition dancerPosition in _dancerPositions)
            dancerPosition.OnDanceupdate(danceTime, beatIndex, beatTime, beatDuration, beatIndex > _beatIndex);

        _beatIndex = beatIndex;
    }

    private void ChangeDancerRole(DancerPosition position, DancerRole role)
    {
        position.SetRole(role);

        string key;
        if (role.group.id.StartsWith(DancerGroup.INACTIVE_ID))
        {
            key = PawnModelDatabase.GetPresetKey(role.id, DancerGroup.INACTIVE_ID, role.Variant);
        }
        else
        {
            key = PawnModelDatabase.GetPresetKey(role.id, "", role.Variant);
            position.Pawn.model.SetText(role.group.id);
        }

        PawnModelPreset preset;
        if (PawnModelDatabase.GetInstance().TryGetPreset(key, out preset))
            position.Pawn.model.SetVisualsFromPreset(preset);
        else
            Debug.LogError($"F({Time.frameCount}): Error in switching dancer {position.DancerIndex}/{_dancerPositions.Length} at set position {position.SetPositionIndex}/{_setLength} role {position.Role.key} to {key} visuals!");
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class DataLoader : MonoBehaviour
{
    public delegate void OnCompletionDelegate();
    public delegate void OnMessageDelegate(string message);

    public OnCompletionDelegate OnCompletionCallback;

    public ActionPresetDatabase _actionPresetDatabase;
    public PawnModelDatabase _pawnModelDatabase;

    private bool _isLoading;
    private bool _isDoneLoading;
    private DanceDatabase _danceDatabase;

    public bool IsLoading { get { return _isLoading; } }
    public bool IsDoneLoading { get { return _isDoneLoading; } }

    private void Awake()
    {
        if (_actionPresetDatabase)
            _actionPresetDatabase.Init();
        if (_pawnModelDatabase)
            _pawnModelDatabase.Init();

        _danceDatabase = DanceDatabase.GetInstance();
    }

    // *************************
    // ******** LOADING ********
    // *************************

    private void OnLoadingComplete()
    {
        _isLoading = false;
        _isDoneLoading = true;

        if (OnCompletionCallback != null)
            OnCompletionCallback.Invoke();
    }

    public void LoadData()
    {
        _isLoading = true;
        StartCoroutine(LoadRoutine());
    }

    private IEnumerator LoadRoutine()
    {
        List<JSONChoreography> danceList = new List<JSONChoreography>();

        yield return FindDances(Application.dataPath + "/Resources/DanceData/", danceList);

        if (danceList.Count > 0)
            yield return ImportDances(danceList);

        OnLoadingComplete();
    }

    private IEnumerator FindDances(string danceDataPath, List<JSONChoreography> danceList)
    {
        Debug.Log($"Looking for dance data in '{danceDataPath}'");

        if (!Directory.Exists(danceDataPath))
            yield break;
        
        EnumerationOptions enumerationOptions = new EnumerationOptions();
        enumerationOptions.RecurseSubdirectories = true;
        string[] jsonFiles = Directory.GetFiles(danceDataPath, "*.json", enumerationOptions);

        Debug.Log($"Found {jsonFiles.Length} dance data files in '{danceDataPath}'");

        if (jsonFiles.Length == 0)
            yield break;

        JSONChoreography jsonData;
        DataPayloadString jsonPayload;

        for (int i = 0; i < jsonFiles.Length; i++)
        {
            jsonPayload = new DataPayloadString(jsonFiles[i].Replace('\\', '/'));

            // TODO: Display progress

            yield return LoadJSON(jsonPayload);

            jsonData = JsonUtility.FromJson<JSONChoreography>(jsonPayload.data);
            if (jsonData != null)
            {
                Debug.Log($"[{i}]: Successfully loaded dance JSON from '{jsonPayload.path}':\n{jsonData.GetDebugString()}");

                jsonData.musicPath = jsonPayload.path.Remove(jsonPayload.path.LastIndexOf('/') + 1) + jsonData.musicPath;
                danceList.Add(jsonData);
            }
            else
            {
                Debug.LogWarning($"[{i}]: Failed to load dance JSON from '{jsonPayload.path}'");
            }
        }
    }

    private IEnumerator ImportDances(List<JSONChoreography> danceList)
    {
        Debug.Log($"Importing {danceList.Count} dances and their music");

        for (int j = 0; j < danceList.Count; j++)
            yield return LoadDanceJSON(danceList[j]);
    }

    public IEnumerator LoadDanceJSON(JSONChoreography jsonData)
    {
        DanceData danceData = ParseDance(jsonData, null);

        // TODO: Display progress

        if (File.Exists(jsonData.musicPath))
        {
            DataPayloadAudio audioPayload = new DataPayloadAudio(jsonData.musicPath, AudioType.MPEG);
            Debug.Log($"Loading music at '{audioPayload.path}'");

            yield return LoadAudioClip(audioPayload);

            if (audioPayload.clip != null)
            {
                danceData.music = audioPayload.clip;
                Debug.Log($"Successfully loaded music at '{audioPayload.path}'");

                _danceDatabase.AddDance(danceData);
            }
            else
            {
                Debug.LogWarning($"Unable to load music at '{audioPayload.path}'");
            }
        }
        else
        {
            Debug.LogWarning($"No music found for '{jsonData.danceName}', at '{jsonData.musicPath}'");
            yield return null;
        }
    }

    public IEnumerator ImportDanceJSON(JSONChoreography jsonData, OnMessageDelegate onProgressDelegate, OnCompletionDelegate onCompletionDelegate, OnMessageDelegate onErrorDelegate)
    {
        DanceData danceData = ParseDance(jsonData, null);

        // TODO: Improve type detection
        AudioType audioType = AudioType.MPEG;
        if (jsonData.musicPath.EndsWith(".ogg"))
        {
            audioType = AudioType.OGGVORBIS;
        }

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(jsonData.musicPath, audioType))
        {
            UnityWebRequestAsyncOperation operation = www.SendWebRequest();

            while (!operation.isDone)
            {
                yield return new WaitForSecondsRealtime(1f);

                if (onProgressDelegate != null)
                {
                    int progress = Mathf.FloorToInt(100f * operation.progress);
                    onProgressDelegate.Invoke($"Loading music file, progress={progress}%");
                }
            }

            if (www.result == UnityWebRequest.Result.Success)
            {
                danceData.music = ((DownloadHandlerAudioClip)www.downloadHandler).audioClip;
                Debug.Log($"Successfully imported music at '{jsonData.musicPath}'");

                _danceDatabase.AddDance(danceData);

                if (onCompletionDelegate != null)
                    onCompletionDelegate.Invoke();
            }
            else
            {
                if (onErrorDelegate != null)
                    onErrorDelegate.Invoke("Unable to load music at given path");

                Debug.LogWarning($"Unable to load music at '{jsonData.musicPath}'");
            }

        }
    }

    private IEnumerator LoadJSON(DataPayloadString payload)
    {
        using (UnityWebRequest fileRequest = UnityWebRequest.Get($"file://{payload.path}"))
        {
            fileRequest.SendWebRequest();

            while (!fileRequest.isDone)
            {
                // TODO: Display progress
                yield return null;
            }

            if (fileRequest.result == UnityWebRequest.Result.Success)
            {
                payload.data = fileRequest.downloadHandler.text;
            }
        }
    }

    private IEnumerator LoadAudioClip(DataPayloadAudio payload)
    {
        using (UnityWebRequest fileRequest = UnityWebRequestMultimedia.GetAudioClip($"file://{payload.path}", payload.type))
        {
            fileRequest.SendWebRequest();

            while (!fileRequest.isDone)
            {
                // TODO: Display progress
                yield return null;
            }

            if (fileRequest.result == UnityWebRequest.Result.Success)
            {
                payload.clip = DownloadHandlerAudioClip.GetContent(fileRequest);
            }
        }
    }

    // *************************
    // ******** PARSING ********
    // *************************

    public DanceData ParseDance(JSONChoreography json, AudioClip musicClip)
    {
        DanceData danceData = new DanceData();

        danceData.danceName = json.danceName;
        danceData.danceType = json.danceType;
        danceData.danceFamily = json.danceFamily != null ? json.danceFamily : "";

        danceData.music = musicClip;
        danceData.firstBeatTime = json.musicFirstBeatTime;
        danceData.bpm = ParseBPM(json.musicBPM, json.musicFirstBeatTime, json.musicBeatTimings);

        danceData.SetGroups(ParseGroups(json.groups));
        danceData.placements = ParseFormation(json.formation, danceData.danceType);
        danceData.parts = ParseChoreography(json.choreography, danceData);

        return danceData;
    }

    private MusicBPM ParseBPM(float initialBPM, float startTime, JSONMusicBeatTiming[] beatTimings)
    {
        MusicBPM musicBPM = new MusicBPM();
        musicBPM.SetInitialBPM(initialBPM);

        if (beatTimings != null && beatTimings.Length > 0)
        {
            int beat = 0;
            float time = startTime;

            JSONMusicBeatTiming timing;
            float bpm, span;

            for (int i = 0; i < beatTimings.Length; i++)
            {
                timing = beatTimings[i];
                span = timing.time - time;
                bpm = 60 * (timing.beat - beat) / span;
                Debug.Log($"Parsed BPM change on beat {timing.beat} @" +
                    $" time: {TimeSpan.FromSeconds(timing.time).ToString("mm'm 'ss's 'fff'ms'")}" +
                    $", span from previous: {TimeSpan.FromSeconds(span).ToString("mm'm 'ss's 'fff'ms'")} = {bpm}bpm");
                if (beat == 0)
                    musicBPM.currentBPM = bpm;
                else
                    musicBPM.AddBPMChange(beat, bpm);
                beat = timing.beat;
                time = timing.time;
            }
        }

        return musicBPM;
    }

    private DancerGroup[] ParseGroups(JSONDancerGroup[] jsonDancerGroups)
    {
        DancerGroup[] dancerGroups = new DancerGroup[jsonDancerGroups.Length];

        DancerGroup group;
        for (int i = 0; i < jsonDancerGroups.Length; i++)
        {
            group = ParseDancerGroup(jsonDancerGroups[i], i);
            dancerGroups[i] = group;
        }

        return dancerGroups;
    }

    private DancerGroup ParseDancerGroup(JSONDancerGroup jsonDancerGroup, int groupIndex)
    {
        DancerGroup dancerGroup = new DancerGroup(jsonDancerGroup.group);
        DancerRole[] groupRoles = new DancerRole[jsonDancerGroup.roles.Length];

        float groupOffset = groupIndex * 0.1f;
        DancerRole role;
        for (int i = 0; i < jsonDancerGroup.roles.Length; i++)
        {
            role = new DancerRole(jsonDancerGroup.roles[i], groupOffset + i * 0.05f);
            role.SetGroup(dancerGroup);
            groupRoles[i] = role;
        }

        dancerGroup.SetRoles(groupRoles);
        return dancerGroup;
    }

    private DancerPlacement[] ParseFormation(JSONDancerPosition[] jsonFormation, string formationType)
    {
        DancerPlacement[] dancerPlacements = new DancerPlacement[jsonFormation.Length];

        JSONDancerPosition position;
        for (int i = 0; i < jsonFormation.Length; i++)
        {
            position = jsonFormation[i];
            if (position.startFacing == null)
                position.startFacing = "uphall"; // Make sure default is uphall

            dancerPlacements[i] = new DancerPlacement(position.role, position.group, position.variant,
                GetDancerPositionInFormation(position.groupPosition, position.rolePosition, formationType),
                GetDancerFacingDirectionInFormation(position.groupPosition, position.rolePosition, position.startFacing, formationType));
        }

        return dancerPlacements;
    }

    private Vector2 GetDancerPositionInFormation(float groupPosition, float rolePosition, string formationType)
    {
        // TODO: Implement other formation types
        switch (formationType)
        {
            default:
                return new Vector2(rolePosition, groupPosition);
        }
    }

    private DanceDirection GetDancerFacingDirectionInFormation(float groupPosition, float rolePosition, string orientation, string formationType)
    {
        // TODO: Implement other formation types
        switch (formationType)
        {
            default:
                return ParseDirection(orientation);
        }
    }

    private DancePart[] ParseChoreography(JSONDancePart[] jsonParts, DanceData danceData)
    {
        DancePart[] danceParts = new DancePart[jsonParts.Length];

        JSONDancePart jsonPart;
        DancePart dancePart;

        for (int i = 0; i < jsonParts.Length; i++)
        {
            jsonPart = jsonParts[i];

            // Make sure optional part fields are initialized
            if (jsonPart.part == null) jsonPart.part = $"Part{i+1}";

            dancePart = ParseDancePart(jsonPart, danceData);
            danceParts[i] = dancePart;
        }

        return danceParts;
    }

    private DancePart ParseDancePart(JSONDancePart jsonPart, DanceData danceData)
    {
        Debug.Log($"Parsing dance part '{jsonPart.part}' at time {jsonPart.time}");

        DancePart dancePart = new DancePart(jsonPart.part, jsonPart.time, null);
        dancePart.actions = ParseDanceActions(jsonPart.actions, dancePart, danceData);

        return dancePart;
    }

    private DanceAction[] ParseDanceActions(JSONDanceAction[] jsonActions, DancePart dancePart, DanceData danceData)
    {
        List<DanceAction> danceActions = new List<DanceAction>(jsonActions.Length);

        JSONDanceAction jsonAction;
        JSONDancerRole jsonRole;
        DanceAction danceAction;
        ActionPreset actionPreset;
        DancerRole dancerRole;
        string key;
        int time;

        for (int i = 0; i < jsonActions.Length; i++)
        {
            jsonAction = jsonActions[i];

            // Make sure optional action fields are initialized
            if (jsonAction.family == null)
                jsonAction.family = danceData.danceFamily;
            if (jsonAction.variant == null)
                jsonAction.variant = "";
            if (jsonAction.part == null)
                jsonAction.part = "";

            key = ActionPresetDatabase.GetPresetKey(jsonAction.family, jsonAction.action, jsonAction.variant);
            if (!_actionPresetDatabase.TryGetPreset(key, out actionPreset))
            {
                Debug.LogError($"Unhandled action type '{key}' found");
                continue;
            }

            for (int j = 0; j <= jsonAction.repeat; j++)
            {
                time = dancePart.time + jsonAction.time + j * jsonAction.duration;
                danceAction = ParseDanceAction(time, jsonAction, actionPreset, dancePart);
                danceActions.Add(danceAction);

                for (int k = 0; k < jsonAction.dancers.Length; k++)
                {
                    jsonRole = jsonAction.dancers[k];

                    if (danceData.TryGetRole(DancerRole.GetRoleKey(jsonRole.group, jsonRole.role), out dancerRole))
                    {
                        Debug.Log($"Adding dance action to role {dancerRole.group.id}.{dancerRole.id}: '{danceAction.key}' T={danceAction.time}, D={danceAction.duration}");
                        dancerRole.AddAction(danceAction.time, danceAction);
                    }
                }
            }
        }

        return danceActions.ToArray();
    }

    private DanceAction ParseDanceAction(int time, JSONDanceAction jsonAction, ActionPreset actionPreset, DancePart dancePart)
    {
        Debug.Log($"Parsing dance action at time {time}, part {jsonAction.part}: {DanceAction.GetActionKey(jsonAction.family, jsonAction.action, jsonAction.variant)}");

        DanceAction danceAction = new DanceAction(
            jsonAction.family,
            jsonAction.action,
            jsonAction.variant,
            jsonAction.part,
            time,
            jsonAction.duration,
            ParseDirection(jsonAction.startFacing),
            jsonAction.movements != null && jsonAction.movements.Length > 0 ? ParseMovement(jsonAction.movements) : null,
            ParsePositionTransitions(jsonAction.positionTransitions, jsonAction.rotationTransitions),
            actionPreset.animation,
            actionPreset.duration);

        return danceAction;
    }

    private DanceMovement ParseMovement(JSONDanceMovement[] jsonMovements)
    {
        Vector3 vector = Vector3.zero;
        DanceVector[] directions = new DanceVector[jsonMovements.Length];

        DanceVector danceVector;
        JSONDanceMovement movement;
        for (int i = 0; i < jsonMovements.Length; i++)
        {
            movement = jsonMovements[i];
            danceVector = new DanceVector(ParseDirection(movement.direction), movement.distance);
            directions[i] = danceVector;
            vector += DanceUtility.GetVectorFromDirection(danceVector.direction) * danceVector.distance;
            Debug.Log($"Parsed dance movement direction {danceVector.direction}, distance {danceVector.distance}");
        }

        Debug.Log($"Parsed dance movement vector {vector.normalized}, distance {vector.magnitude}");
        return new DanceMovement(directions, vector);
    }

    private DanceActionTransitions ParsePositionTransitions(JSONDanceActionPositionTransition[] positions, JSONDanceActionRotationTransition[] rotations)
    {
        if ((positions == null || positions.Length == 0) && (rotations == null || rotations.Length == 0))
            return null;

        DanceActionTransitions transitions = new DanceActionTransitions();

        if (positions != null)
        {
            JSONDanceActionPositionTransition jsonPos;
            JSONDanceMovement movement;
            Vector3 vector, partial;

            for (int i = 0; i < positions.Length; i++)
            {
                jsonPos = positions[i];
                vector = Vector3.zero;

                for (int k = 0; k < jsonPos.vectors.Length; k++)
                {
                    movement = jsonPos.vectors[k];
                    partial = DanceUtility.GetVectorFromDirection(ParseDirection(movement.direction)) * movement.distance;
                    vector += partial;
                    Debug.Log($"Parsing position transition[{i}] time={jsonPos.time}, duration={jsonPos.duration}, partial vector: {movement.direction}^{movement.distance}");
                }

                Debug.Log($"Parsing position transition[{i}] time={jsonPos.time}, duration={jsonPos.duration}, vector={vector:F3}");
                transitions.AddTransition(new DanceActionPositionTransition(jsonPos.time, jsonPos.duration, vector));
            }
        }

        if (rotations != null)
        {
            JSONDanceActionRotationTransition jsonRot;
            for (int i = 0; i < rotations.Length; i++)
            {
                jsonRot = rotations[i];
                Debug.Log($"Parsing rotation transition[{i}] time={jsonRot.time}, duration={jsonRot.duration}, direction={jsonRot.direction}, amount={jsonRot.amount}");
                transitions.AddTransition(new DanceActionRotationTransition(jsonRot.time, jsonRot.duration, ParseDirection(jsonRot.direction), jsonRot.amount));
            }
        }

        return transitions;
    }

    private DanceDirection ParseDirection(string direction)
    {
        switch (direction.ToLower())
        {
            case "up":
            case "uphall":
            case "forward":
                return DanceDirection.Up;
            case "down":
            case "downhall":
            case "backward":
                return DanceDirection.Down;
            case "left":
                return DanceDirection.Left;
            case "right":
                return DanceDirection.Right;
            case "upleft":
                return DanceDirection.UpLeft;
            case "upright":
                return DanceDirection.UpRight;
            case "downright":
                return DanceDirection.DownRight;
            case "downleft":
                return DanceDirection.DownLeft;
            case "cw":
            case "clockwise":
                return DanceDirection.CW;
            case "ccw":
            case "counterclockwise":
                return DanceDirection.CCW;
            default:
                return DanceDirection.Up;
        }
    }
}

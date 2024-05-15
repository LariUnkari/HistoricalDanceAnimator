using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class DataLoader : MonoBehaviour
{
    public delegate void OnCompletionDelegate();

    public OnCompletionDelegate OnCompletionCallback;

    public ActionPresetDatabase _actionPresetDatabase;
    public PawnModelDatabase _pawnModelDatabase;

    private void Awake()
    {
        if (_actionPresetDatabase)
            _actionPresetDatabase.Init();
        if (_pawnModelDatabase)
            _pawnModelDatabase.Init();
    }

    // *************************
    // ******** LOADING ********
    // *************************

    private void OnLoadingComplete()
    {
        if (OnCompletionCallback != null)
            OnCompletionCallback.Invoke();
    }

    public void LoadData()
    {
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

        DanceDatabase database = DanceDatabase.GetInstance();

        JSONChoreography jsonData;
        DanceData danceData;
        DataPayloadAudio audioPayload;

        for (int j = 0; j < danceList.Count; j++)
        {
            jsonData = danceList[j];
            danceData = ParseDance(jsonData, null);

            // TODO: Display progress

            if (File.Exists(jsonData.musicPath))
            {
                audioPayload = new DataPayloadAudio(jsonData.musicPath, AudioType.MPEG);
                Debug.Log($"Loading music at '{audioPayload.path}'");

                yield return LoadAudioClip(audioPayload);

                if (audioPayload.clip != null)
                {
                    danceData.music = audioPayload.clip;
                    Debug.Log($"Successfully loaded music at '{audioPayload.path}'");
                }
                else
                {
                    Debug.LogWarning($"Unable to load music at '{audioPayload.path}'");
                }
            }
            else
            {
                Debug.LogWarning("No music found for '{jsonData.danceName}', at '{jsonData.musicPath}'");
                yield return null;
            }

            database.AddDance(danceData);
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
        danceData.bpm = json.musicBPM;
        danceData.firstBeatTime = json.musicFirstBeatTime;

        danceData.SetGroups(ParseGroups(json.groups));
        danceData.placements = ParseFormation(json.formation, danceData.danceType);
        danceData.parts = ParseChoreography(json.choreography, danceData);

        return danceData;
    }

    private DancerGroup[] ParseGroups(JSONDancerGroup[] jsonDancerGroups)
    {
        DancerGroup[] dancerGroups = new DancerGroup[jsonDancerGroups.Length];

        DancerGroup group;
        for (int i = 0; i < jsonDancerGroups.Length; i++)
        {
            group = ParseDancerGroup(jsonDancerGroups[i]);
            dancerGroups[i] = group;
        }

        return dancerGroups;
    }

    private DancerGroup ParseDancerGroup(JSONDancerGroup jsonDancerGroup)
    {
        DancerGroup dancerGroup = new DancerGroup(jsonDancerGroup.group);
        DancerRole[] groupRoles = new DancerRole[jsonDancerGroup.roles.Length];

        DancerRole role;
        for (int i = 0; i < jsonDancerGroup.roles.Length; i++)
        {
            role = new DancerRole(jsonDancerGroup.roles[i]);
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
            dancerPlacements[i] = new DancerPlacement(position.role, position.group, position.variant,
                GetDancerPositionInFormation(position.groupPosition, position.rolePosition, formationType));
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
        DanceAction[] danceActions = new DanceAction[jsonActions.Length];

        JSONDanceAction jsonAction;
        JSONDancerRole jsonRole;
        DanceAction danceAction;
        ActionPreset actionPreset;
        DancerRole dancerRole;
        string key;

        for (int i = 0; i < jsonActions.Length; i++)
        {
            jsonAction = jsonActions[i];

            // Make sure optional action fields are initialized
            if (jsonAction.family == null || jsonAction.family.Length == 0)
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

            danceAction = ParseDanceAction(jsonAction, actionPreset, dancePart);
            danceActions[i] = danceAction;

            for (int j = 0; j < jsonAction.dancers.Length; j++)
            {
                jsonRole = jsonAction.dancers[j];

                if (danceData.TryGetRole(DancerRole.GetRoleKey(jsonRole.group, jsonRole.role), out dancerRole))
                {
                    Debug.Log($"Adding dance action to role {dancerRole.group.id}.{dancerRole.id}");
                    dancerRole.AddAction(danceAction.time, danceAction);
                }
            }
        }

        return danceActions;
    }

    private DanceAction ParseDanceAction(JSONDanceAction jsonAction, ActionPreset actionPreset, DancePart dancePart)
    {
        Debug.Log($"Parsing dance action at time {jsonAction.time}, part {jsonAction.part}: {DanceAction.GetActionKey(jsonAction.family, jsonAction.action, jsonAction.variant)}");

        DanceAction danceAction = new DanceAction(
            jsonAction.family,
            jsonAction.action,
            jsonAction.variant,
            jsonAction.part,
            jsonAction.time + dancePart.time,
            jsonAction.duration,
            ParseDirection(jsonAction.startFacing),
            jsonAction.movements != null && jsonAction.movements.Length > 0 ? ParseMovement(jsonAction.movements) : null,
            ParseTransitions(jsonAction.transitions),
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

    private DanceActionTransition[] ParseTransitions(JSONDanceActionTransition[] jsonTransitions)
    {
        if (jsonTransitions == null || jsonTransitions.Length == 0)
        {
            return new DanceActionTransition[0];
        }

        DanceActionTransition[] transitions = new DanceActionTransition[jsonTransitions.Length];

        JSONDanceActionTransition json;
        for (int i = 0; i < jsonTransitions.Length; i++)
        {
            json = jsonTransitions[i];
            Debug.Log($"Parsing transition[{i}] time={json.time}, duration={json.duration}, direction={json.direction}, amount={json.amount}");

            transitions[i] = new DanceActionTransition(
                json.time,
                json.duration,
                ParseDirection(json.direction),
                json.amount);
        }

        return transitions;
    }

    private DanceDirection ParseDirection(string direction)
    {
        switch (direction.ToLower())
        {
            case "up":
            case "uphall":
                return DanceDirection.Up;
            case "down":
            case "downhall":
                return DanceDirection.Down;
            case "left":
                return DanceDirection.Left;
            case "right":
                return DanceDirection.Right;
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

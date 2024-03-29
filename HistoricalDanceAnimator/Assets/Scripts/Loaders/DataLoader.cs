using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class DataLoader : MonoBehaviour
{
    public delegate void OnCompletionDelegate();

    public OnCompletionDelegate OnCompletionCallback;

    public PawnModelDatabase pawnModelDatabase;

    private void Awake()
    {
        if (pawnModelDatabase)
            pawnModelDatabase.Init();
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

        danceData.music = musicClip;
        danceData.bpm = json.musicBPM;
        danceData.firstBeatTime = json.musicFirstBeatTime;

        danceData.SetGroups(ParseGroups(json.groups));
        danceData.formation = ParseFormation(json.formation, danceData.danceType);
        danceData.actions = ParseChoreography(json.choreography);

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

    private DanceFormation ParseFormation(JSONDancerPosition[] jsonFormation, string formationType)
    {
        DancerPosition[] dancerPositions = new DancerPosition[jsonFormation.Length];

        JSONDancerPosition position;
        for (int i = 0; i < jsonFormation.Length; i++)
        {
            position = jsonFormation[i];
            dancerPositions[i] = new DancerPosition(position.role, position.group, position.variant,
                GetDancerPositionInFormation(position.groupPosition, position.rolePosition, formationType));
        }

        return new DanceFormation(dancerPositions);
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

    private DanceAction[] ParseChoreography(JSONDanceAction[] jsonActions)
    {
        DanceAction[] danceActions = new DanceAction[jsonActions.Length];

        for (int i = 0; i < jsonActions.Length; i++)
        {
            danceActions[i] = ParseDanceAction(jsonActions[i]);
        }

        return danceActions;
    }

    private DanceAction ParseDanceAction(JSONDanceAction json)
    {
        DanceAction danceAction = new DanceAction(
            json.action,
            json.variant,
            json.duration,
            ParseDirection(json.startFacing),
            ParseDirection(json.endFacing),
            ParseMovement(json.movements),
            null);

        return danceAction;
    }

    private DanceMovement ParseMovement(JSONDanceMovement[] jsonMovements)
    {
        DanceMoveDirection[] directions = new DanceMoveDirection[jsonMovements.Length];

        JSONDanceMovement movement;
        for (int i = 0; i < jsonMovements.Length; i++)
        {
            movement = jsonMovements[i];
            directions[i] = new DanceMoveDirection(ParseDirection(movement.axis), movement.distance);
        }

        return new DanceMovement(directions);
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

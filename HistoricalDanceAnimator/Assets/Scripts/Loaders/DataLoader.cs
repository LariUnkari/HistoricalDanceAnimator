using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class DataLoader : MonoBehaviour
{
    public delegate void OnCompletionDelegate();

    public OnCompletionDelegate OnCompletionCallback;

    public void LoadData()
    {
        StartCoroutine(LoadRoutine());
    }

    private IEnumerator LoadRoutine()
    {
        List<DanceJSON> danceList = new List<DanceJSON>();

        yield return FindDances(Application.dataPath + "/Resources/DanceData/", danceList);

        if (danceList.Count > 0)
            yield return ImportDances(danceList);

        OnLoadingComplete();
    }

    private IEnumerator FindDances(string danceDataPath, List<DanceJSON> danceList)
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

        DanceJSON jsonData;
        DataPayloadJSON jsonPayload;

        for (int i = 0; i < jsonFiles.Length; i++)
        {
            jsonPayload = new DataPayloadJSON(jsonFiles[i].Replace('\\', '/'));

            // TODO: Display progress

            yield return LoadJSON(jsonPayload);

            jsonData = JsonUtility.FromJson<DanceJSON>(jsonPayload.data);
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

    private IEnumerator ImportDances(List<DanceJSON> danceList)
    {
        Debug.Log($"Importing {danceList.Count} dances and their music");

        DanceDatabase database = DanceDatabase.GetInstance();

        DanceJSON jsonData;
        DanceData danceData;
        DataPayloadAudio audioPayload;

        for (int j = 0; j < danceList.Count; j++)
        {
            jsonData = danceList[j];
            danceData = DanceData.Create(jsonData, null);

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

    private IEnumerator LoadJSON(DataPayloadJSON payload)
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

    private void OnLoadingComplete()
    {
        if (OnCompletionCallback != null)
            OnCompletionCallback.Invoke();
    }
}

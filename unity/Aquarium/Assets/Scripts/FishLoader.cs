using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

// Leest fish.json in en meldt elke vis via het event FishLoaded.
// - In de editor: uit Assets/StreamingAssets/fish.json
// - In de WebGL-build: de fish.json die naast de build op de website staat
public class FishLoader : MonoBehaviour
{
    // Andere scripts luisteren naar deze event om te weten dat er vissen geladen zijn.
    public event Action<FishData> FishLoaded;

    private IEnumerator Start()
    {
        string json = null;

#if UNITY_WEBGL && !UNITY_EDITOR
        // Online: fish.json staat op dezelfde site als de build.
        // ?t=... zorgt dat de browser geen oude kopie uit de cache gebruikt.
        string url = new Uri(new Uri(Application.absoluteURL), "fish.json").ToString()
                     + "?t=" + DateTime.UtcNow.Ticks;
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
                json = request.downloadHandler.text;
            else
                Debug.LogWarning("fish.json kon niet geladen worden: " + request.error);
        }
#else
        // Editor of desktop: lees de kopie uit StreamingAssets.
        string path = Path.Combine(Application.streamingAssetsPath, "fish.json");
        if (File.Exists(path))
            json = File.ReadAllText(path);
        else
            Debug.LogWarning("fish.json niet gevonden op " + path);
        yield return null;
#endif

        if (json == null)
            yield break; // niets geladen: het aquarium draait gewoon door

        FishList list = JsonUtility.FromJson<FishList>(json);
        if (list == null || list.fish == null)
        {
            Debug.LogWarning("fish.json heeft niet de verwachte vorm.");
            yield break;
        }

        Debug.Log($"fish.json geladen ({list.generated}): {list.fish.Count} vis(sen).");
        foreach (FishData fish in list.fish)
        {
            Debug.Log($"Vis gevonden: {fish.name}, gemaakt door {fish.maker}");
            FishLoaded?.Invoke(fish);
            yield return null; // één vis per frame, zodat het beeld niet hapert
        }
    }
}

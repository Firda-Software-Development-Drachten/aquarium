using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

// Leest fish.json in en meldt elke NIEUWE vis via het event FishLoaded.
// Elke 'reloadSeconds' seconden kijkt het opnieuw, zodat vissen die na een
// merge op de site komen vanzelf verschijnen, zonder de pagina te verversen.
// - In de editor: uit Assets/StreamingAssets/fish.json
// - In de WebGL-build: de fish.json die naast de build op de website staat
public class FishLoader : MonoBehaviour
{
    // Andere scripts luisteren hiernaar, bijv. de FishFactory.
    public event Action<FishData> FishLoaded;

    [Tooltip("Hoe vaak fish.json opnieuw wordt opgehaald, in seconden")]
    public float reloadSeconds = 60f;

    private readonly HashSet<string> knownIds = new HashSet<string>();
    private string lastGenerated = null;

    private IEnumerator Start()
    {
        while (true)
        {
            yield return LoadOnce();
            yield return new WaitForSeconds(reloadSeconds);
        }
    }

    private IEnumerator LoadOnce()
    {
        string json = null;

#if UNITY_WEBGL && !UNITY_EDITOR
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
        string path = Path.Combine(Application.streamingAssetsPath, "fish.json");
        if (File.Exists(path))
            json = File.ReadAllText(path);
        else
            Debug.LogWarning("fish.json niet gevonden op " + path);
        yield return null;
#endif

        if (json == null)
            yield break; // niets geladen: de vissen die er al zijn zwemmen gewoon door

        FishList list = JsonUtility.FromJson<FishList>(json);
        if (list == null || list.fish == null)
        {
            Debug.LogWarning("fish.json heeft niet de verwachte vorm.");
            yield break;
        }

        // Niets veranderd sinds de vorige keer? Dan hoeven we niets te doen.
        if (list.generated == lastGenerated)
            yield break;
        lastGenerated = list.generated;

        foreach (FishData fish in list.fish)
        {
            if (knownIds.Contains(fish.id))
                continue; // deze vis zwemt er al

            knownIds.Add(fish.id);
            Debug.Log($"Nieuwe vis: {fish.name}, gemaakt door {fish.maker}");
            FishLoaded?.Invoke(fish);
            yield return null; // één vis per frame, zodat het beeld niet hapert
        }
    }
}

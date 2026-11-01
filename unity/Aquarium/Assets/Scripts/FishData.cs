using System;
using System.Collections.Generic;

// Deze klassen volgen precies de vorm van fish.json.
// JsonUtility vult ze automatisch, zolang de veldnamen gelijk zijn aan de JSON.

[Serializable]
public class FishColors
{
    public string body;     // bijv. "#ff8800"
    public string fins;
    public string pattern;
}

[Serializable]
public class FishData
{
    public string id;       // de bestandsnaam, bijv. "billy"
    public string name;
    public string maker;
    public string body;     // "round" of "long"
    public string fins;     // "small" of "pointy"
    public string pattern;  // "none" of "stripes"
    public FishColors colors;
    public float size;
    public float speed;
    public string behavior; // "calm" of "curious"
}

// De buitenste laag van fish.json: { "generated": "...", "fish": [ ... ] }
[Serializable]
public class FishList
{
    public string generated;
    public List<FishData> fish;
}

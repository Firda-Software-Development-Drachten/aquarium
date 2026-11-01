using UnityEngine;

// Bouwt een vis uit simpele vormen zodra de FishLoader een vis meldt.
// Zet dit script op hetzelfde GameObject als de FishLoader.
// De vis kijkt naar rechts (+x). Alle onderdelen zijn bollen en blokjes:
// echte modellen zijn een mooie uitbreiding voor later.
[RequireComponent(typeof(FishLoader))]
public class FishFactory : MonoBehaviour
{
    [Tooltip("Midden van de bak (het lege object Tank)")]
    public Transform tank;

    [Tooltip("Grootte van het gebied waarin vissen verschijnen")]
    public Vector3 spawnArea = new Vector3(12f, 5f, 3f);

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly Color FallbackColor = Color.gray;

    private void Awake()
    {
        GetComponent<FishLoader>().FishLoaded += BuildFish;
    }

    private void BuildFish(FishData data)
    {
        // Leeg hoofdobject: hierop komen straks ook de zwemscripts.
        GameObject fish = new GameObject("Fish_" + data.id);
        fish.transform.SetParent(transform);
        fish.transform.position = RandomSpawnPosition();

        Color bodyColor = ParseColor(data.colors?.body, data.id);
        Color finColor = ParseColor(data.colors?.fins, data.id);
        Color patternColor = ParseColor(data.colors?.pattern, data.id);

        // Lichaam: rond of langwerpig.
        Vector3 bodySize;
        switch (data.body)
        {
            case "round": bodySize = new Vector3(1.0f, 0.8f, 0.6f); break;
            case "long":  bodySize = new Vector3(1.6f, 0.6f, 0.5f); break;
            default:
                Debug.LogWarning($"Vis '{data.id}': onbekend lichaam '{data.body}', ik gebruik 'round'.");
                bodySize = new Vector3(1.0f, 0.8f, 0.6f);
                break;
        }
        AddPart(fish, PrimitiveType.Sphere, "Body", Vector3.zero, bodySize, Vector3.zero, bodyColor);

        // Vinnen: klein of puntig. Een staart achteraan en een rugvin bovenop.
        float finScale;
        switch (data.fins)
        {
            case "small":  finScale = 0.35f; break;
            case "pointy": finScale = 0.6f; break;
            default:
                Debug.LogWarning($"Vis '{data.id}': onbekende vinnen '{data.fins}', ik gebruik 'small'.");
                finScale = 0.35f;
                break;
        }
        // De staart groeit mee met de hoogte van het lichaam,
        // zodat een smalle vis (zoals 'long') geen enorme staart krijgt.
        float tailScale = finScale * bodySize.y * 0.8f;
        float back = -bodySize.x / 2f;
        AddPart(fish, PrimitiveType.Cube, "Tail",
            new Vector3(back - tailScale * 0.4f, 0f, 0f),
            new Vector3(tailScale, tailScale, 0.05f), new Vector3(0f, 0f, 45f), finColor);
        AddPart(fish, PrimitiveType.Cube, "TopFin",
            new Vector3(0f, bodySize.y / 2f, 0f),
            new Vector3(finScale * 0.7f, finScale * 0.7f, 0.04f), new Vector3(0f, 0f, 45f), finColor);

        // Patroon: geen of strepen.
        switch (data.pattern)
        {
            case "none": break;
            case "stripes":
                foreach (float x in new[] { -0.2f, 0.1f })
                {
                    AddPart(fish, PrimitiveType.Sphere, "Stripe",
                        new Vector3(x * bodySize.x, 0f, 0f),
                        new Vector3(0.1f * bodySize.x, bodySize.y, bodySize.z), Vector3.zero, patternColor);
                }
                break;
            default:
                Debug.LogWarning($"Vis '{data.id}': onbekend patroon '{data.pattern}', ik gebruik 'none'.");
                break;
        }

        // Ogen aan beide kanten.
        foreach (float side in new[] { -1f, 1f })
        {
            AddPart(fish, PrimitiveType.Sphere, "Eye",
                new Vector3(bodySize.x * 0.3f, bodySize.y * 0.12f, side * bodySize.z * 0.4f),
                Vector3.one * 0.12f, Vector3.zero, Color.black);
        }

        // Grootte uit het visbestand (0.75, 1.0 of 1.5).
        float size = data.size > 0f ? data.size : 1f;
        fish.transform.localScale = Vector3.one * size;
    }

    // Maakt één onderdeel als kind van de vis, zonder collider, in de juiste kleur.
    private void AddPart(GameObject fish, PrimitiveType shape, string partName,
                         Vector3 position, Vector3 scale, Vector3 rotation, Color color)
    {
        GameObject part = GameObject.CreatePrimitive(shape);
        part.name = partName;
        Destroy(part.GetComponent<Collider>()); // vissen hoeven nergens tegenaan te botsen
        part.transform.SetParent(fish.transform, false);
        part.transform.localPosition = position;
        part.transform.localScale = scale;
        part.transform.localEulerAngles = rotation;

        // Kleur via een MaterialPropertyBlock: geen nieuw materiaal per vis.
        var block = new MaterialPropertyBlock();
        block.SetColor(BaseColorId, color);
        part.GetComponent<Renderer>().SetPropertyBlock(block);
    }

    private Color ParseColor(string hex, string fishId)
    {
        if (hex != null && ColorUtility.TryParseHtmlString(hex, out Color color))
            return color;
        Debug.LogWarning($"Vis '{fishId}': ongeldige kleur '{hex}', ik gebruik grijs.");
        return FallbackColor;
    }

    private Vector3 RandomSpawnPosition()
    {
        Vector3 center = tank != null ? tank.position : Vector3.zero;
        return center + new Vector3(
            Random.Range(-spawnArea.x, spawnArea.x) / 2f,
            Random.Range(-spawnArea.y, spawnArea.y) / 2f,
            Random.Range(-spawnArea.z, spawnArea.z) / 2f);
    }
}

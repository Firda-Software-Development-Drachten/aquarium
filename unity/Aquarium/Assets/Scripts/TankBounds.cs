using UnityEngine;

// De grenzen van de bak waarin de vissen zwemmen.
// Zet dit script op het lege object Tank. In de Scene-weergave zie je een
// blauw kader: daarbinnen blijven de vissen.
public class TankBounds : MonoBehaviour
{
    [Tooltip("Breedte, hoogte en diepte van het zwemgebied")]
    public Vector3 size = new Vector3(14f, 6f, 4f);

    // Een willekeurig punt binnen de bak.
    public Vector3 RandomPoint()
    {
        Vector3 half = size / 2f;
        return transform.position + new Vector3(
            Random.Range(-half.x, half.x),
            Random.Range(-half.y, half.y),
            Random.Range(-half.z, half.z));
    }

    // Tekent het kader in de editor (niet zichtbaar in het spel).
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, size);
    }
}

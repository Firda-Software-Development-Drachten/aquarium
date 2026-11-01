using UnityEngine;

// Laat een vis rustig rondzwemmen binnen de bak.
// De vis kiest een willekeurig doel, zwemt ernaartoe en kiest dan een nieuw doel.
public class FishSwimmer : MonoBehaviour
{
    public TankBounds tank;
    public float speed = 1f;           // uit het visbestand: 0.75, 1.0 of 1.5
    public string behavior = "calm";

    private const float BaseSpeed = 1.2f;   // meter per seconde bij speed 1.0
    private Vector3 target;

    private void Start()
    {
        if (behavior != "calm")
        {
            // 'curious' is nog niet gebouwd: een mooi issue voor studenten.
            Debug.LogWarning($"{name}: gedrag '{behavior}' bestaat nog niet, ik gebruik 'calm'.");
        }
        target = tank.RandomPoint();
    }

    private void Update()
    {
        // Zwem in een rechte lijn naar het doel.
        transform.position = Vector3.MoveTowards(
            transform.position, target, speed * BaseSpeed * Time.deltaTime);

        // Kijk naar links of rechts, afhankelijk van de zwemrichting.
        // (Netjes draaien in de zwemrichting is een issue voor studenten.)
        float direction = target.x - transform.position.x;
        if (Mathf.Abs(direction) > 0.01f)
            transform.rotation = Quaternion.Euler(0f, direction > 0f ? 0f : 180f, 0f);

        // Doel bereikt? Kies een nieuw doel.
        if (Vector3.Distance(transform.position, target) < 0.1f)
            target = tank.RandomPoint();
    }
}

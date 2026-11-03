using System;
using UnityEngine;

// Laat een vis rustig rondzwemmen binnen de bak.
// De vis kiest een willekeurig doel, zwemt ernaartoe en kiest dan een nieuw doel.

public class FishSwimmer : MonoBehaviour
{
    public TankBounds tank;
    public float speed = 1f;           // uit het visbestand: 0.75, 1.0 of 1.5
    public string behavior = "calm";

    private const float BaseSpeed = 0.2f;
    private Vector3 target;

    private Vector3 startPoint;
    bool halfway = false;

    private void Start()
    {
        if (behavior != "calm")
        {
            // 'curious' is nog niet gebouwd
            Debug.LogWarning($"{name}: gedrag '{behavior}' bestaat nog niet, ik gebruik 'calm'.");
        }
        target = tank.RandomPoint();
    }

    private void Update()
    {
        // Zwem in een rechte lijn naar het doel.

        float distanceX =   target.x - transform.position.x;
        float distanceY = target.y = transform.position.y;
        float angle = MathF.Atan2( distanceX, distanceY );



        //transform.rotation = Quaternion.Euler(0f, transform.rotation.y, angle);
        if (!halfway)
        {
            halfway = Vector3.Distance(transform.position, Vector3.Lerp(startPoint, target, 0.5f)) < 0.1f; // 50%
        }
        else
        {
            startPoint = transform.position;
            target = tank.RandomPoint();
            halfway = false;
        }


            transform.position = Vector3.Lerp(
                transform.position, target, speed * BaseSpeed * Time.deltaTime
                );

        // Kijk naar links of rechts, afhankelijk van de zwemrichting.
        // (Netjes draaien in de zwemrichting is een issue voor studenten.)
        
        float direction = target.x - transform.position.x;
        if (Mathf.Abs(direction) > 0.01f)
        {
            transform.rotation = Quaternion.Euler(0f, direction > 0f ? 0f : 180f, 0);
        }

            // Doel bereikt? Kies een nieuw doel.
        float distance = Vector3.Distance(transform.position, target);
        if (distance < 0.1f)
        {
            startPoint = transform.position;
            target = tank.RandomPoint();
            halfway = false;
        }
            
    }
}

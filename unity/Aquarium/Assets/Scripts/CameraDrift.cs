using UnityEngine;
using System;

public class CameraDrift: MonoBehaviour {

    private FishFactory fishFactory;
    private GameObject currentFish;

    private float timer = 0.0f;

    private float xVelocity = 0.0f;
    private float yVelocity = 0.0f;

    public float smoothTime = 0.5f;
    public float fishSwitchTime = 7.5f;
    public float minY = -0.25f;

    void Start() {

        fishFactory = FindFirstObjectByType<FishFactory>();

    }

    void Update() {

        timer += Time.deltaTime;

        if (timer >= fishSwitchTime) {

            GameObject newFish = null;
            do { 

                newFish = fishFactory.GetRandomFish(); 

            } while (

                currentFish != null &&
                newFish == currentFish &&
                fishFactory.fishList.Count > 1

            );

            currentFish = newFish;
            timer = 0.0f;

        }

        if (currentFish != null) {

            Vector2 fishPos = new Vector2(currentFish.transform.position.x, currentFish.transform.position.y);
            Vector2 cameraPos = new Vector2(transform.position.x, transform.position.y);

            float distanceToFish = Vector2.Distance(cameraPos, fishPos);
            float criticalDistance = 1.5f;

            float currentSmoothTime = smoothTime;

            if (distanceToFish < criticalDistance) { 

                currentSmoothTime = smoothTime / (((criticalDistance - distanceToFish) / criticalDistance) * 5.0f);

            }

            float xPos = Mathf.SmoothDamp(cameraPos.x, fishPos.x, ref xVelocity, currentSmoothTime);
            float yPos = Mathf.Clamp(Mathf.SmoothDamp(cameraPos.y, fishPos.y, ref yVelocity, currentSmoothTime), minY, Int32.MaxValue);

            transform.position = new Vector3(xPos, yPos, transform.position.z);

        }

    }


}

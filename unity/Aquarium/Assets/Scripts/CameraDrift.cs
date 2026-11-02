using UnityEngine;
using System.Collections.Generic;

public class CameraDrift: MonoBehaviour {

    private FishFactory fishFactory;
    private GameObject currentFish;

    private float timer = 0.0f;

    private float xVelocity = 0.0f;
    private float yVelocity = 0.0f;

    public float smoothTime = 0.5f;
    public float fishSwitchTime = 7.5f;

    void Start() {

        fishFactory = FindFirstObjectByType<FishFactory>();

    }

    void Update() {

        timer += Time.deltaTime;

        if (timer >= fishSwitchTime) {

            GameObject newFish = null;
            do { 

                newFish = GetRandomFish(); 

            } while (newFish == currentFish && currentFish != null);

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
            float yPos = Mathf.SmoothDamp(cameraPos.y, fishPos.y, ref yVelocity, currentSmoothTime);

            transform.position = new Vector3(xPos, yPos, transform.position.z);

        }

    }

    GameObject GetRandomFish() {

        List<Transform> fishList = new List<Transform>();

        foreach (Transform fish in fishFactory.transform) {

            fishList.Add(fish);

        }

        if (fishList.Count == 0) {

            return null;

        } else {

            return fishList[Random.Range(0, fishList.Count)].gameObject;

        }

    }

}

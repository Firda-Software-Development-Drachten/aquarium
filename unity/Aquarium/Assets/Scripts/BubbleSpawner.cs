using System.Collections;
using UnityEngine;

public class BubbleSpawner : MonoBehaviour
{
    [Tooltip("Size of the floor to spawn from")]
    [SerializeField] private Vector2 floorSize = new Vector2(20f, 6f);
    [SerializeField] private float floorHeight = 0f;
    [SerializeField] private ParticleSystem emitterPrefab;
    [SerializeField] private float minLifetime = 10f;
    [SerializeField] private float maxLifetime = 60f;

    private readonly ParticleSystem[] emitters = new ParticleSystem[2];

    private void OnEnable()
    {
        for (int i = 0; i < emitters.Length; i++)
        {
            if (emitters[i] == null)
            {
                emitters[i] = CreateEmitter(i);
            }

            emitters[i].Play(false);
            StartCoroutine(RunEmitter(emitters[i], Random.Range(0f, minLifetime)));
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        foreach (ParticleSystem emitter in emitters)
        {
            if (emitter != null)
            {
                emitter.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }

    private IEnumerator RunEmitter(ParticleSystem emitter, float initialDelay)
    {
        if (initialDelay > 0f)
        {
            yield return new WaitForSeconds(initialDelay);
        }

        int emitType = Random.Range(0, 3);

        while (true)
        {
            emitter.transform.position = new Vector3(
                Random.Range(-0.5f, 0.5f) * floorSize.x,
                floorHeight,
                Random.Range(-0.5f, 0.5f) * floorSize.y);

            yield return new WaitForSeconds(1f);

            float stopTime = Time.time + Random.Range(minLifetime, maxLifetime);

            while (Time.time < stopTime)
            {
                // Choose new spawn frequency
                emitType = (emitType + Random.Range(1, 3)) % 3;

                float burstTime = Mathf.Min(
                    Time.time + Random.Range(0f, 0.5f * minLifetime),
                    stopTime);

                while (Time.time < burstTime)
                {
                    emitter.Emit(1);
                    float delay = Random.Range(emitType + 0.01f, 2f * emitType + 0.5f);
                    yield return new WaitForSeconds(Mathf.Min(delay, burstTime - Time.time));
                }

                float remainingTime = stopTime - Time.time;
                if (remainingTime > 0f)
                {
                    float pause = Random.Range(1f, 3f);
                    yield return new WaitForSeconds(Mathf.Min(pause, remainingTime));
                }
            }
        }
    }

    private ParticleSystem CreateEmitter(int index)
    {
        ParticleSystem emitter = Instantiate(emitterPrefab, transform, false);
        emitter.name = $"Bubble emitter {index + 1}";
        return emitter;
    }
}
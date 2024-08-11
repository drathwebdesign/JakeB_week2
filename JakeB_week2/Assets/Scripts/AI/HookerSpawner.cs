using System.Collections.Generic;
using UnityEngine;

public class HookerSpawner : MonoBehaviour {
    public GameObject hookerPrefab;
    public int minHookers = 1;  // Minimum number of hookers on the map
    public float spawnRadius = 5f;  // Radius around the spawn point to place hookers
    public float respawnInterval = 60f;  // Time interval to check for respawning hookers

    private List<Transform> spawnPoints;
    private List<HookerAI> activeHookers;

    void Start() {
        spawnPoints = new List<Transform>();
        activeHookers = new List<HookerAI>();

        // Find all spawn points in the scene with the "HookerSpawnPoint" tag
        GameObject[] points = GameObject.FindGameObjectsWithTag("HookerSpawnPoint");
        foreach (GameObject point in points) {
            spawnPoints.Add(point.transform);
        }

        if (spawnPoints.Count == 0) {
            Debug.LogError("No spawn points found! Please add objects with the 'HookerSpawnPoint' tag.");
        }

        HookerAI.OnHookerDeath += HandleHookerDeath;
        InvokeRepeating(nameof(CheckAndRespawnHookers), 0f, respawnInterval);
    }

    void OnDestroy() {
        HookerAI.OnHookerDeath -= HandleHookerDeath;
    }

    void CheckAndRespawnHookers() {
        // Ensure there are always at least minHookers on the map
        while (activeHookers.Count < minHookers) {
            SpawnHooker();
        }
    }

    void SpawnHooker() {
        // Select a random spawn point
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];

        Vector3 spawnPosition = spawnPoint.position + Random.insideUnitSphere * spawnRadius;
        spawnPosition.y = spawnPoint.position.y;

        GameObject hookerObject = Instantiate(hookerPrefab, spawnPosition, Quaternion.identity);
        HookerAI hooker = hookerObject.GetComponent<HookerAI>();
        if (hooker != null) {
            activeHookers.Add(hooker);
        } else {
            Debug.LogWarning("HookerPrefab does not have a HookerAI component.");
        }
    }

    void HandleHookerDeath(HookerAI hooker) {
        if (activeHookers.Contains(hooker)) {
            activeHookers.Remove(hooker);
        }
    }
}

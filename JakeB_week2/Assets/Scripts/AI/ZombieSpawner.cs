using System.Collections.Generic;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour {
    public GameObject zombiePrefab;
    public int minZombies = 4;  // Minimum number of zombies across the entire map
    public int maxZombies = 300;  // Maximum number of zombies allowed across the entire map
    public float spawnRadius = 12f;  // Radius around the spawn point to place zombies
    public float respawnInterval = 60f;  // Time interval to check for respawning zombies

    private List<Transform> spawnPoints;
    private List<ZombieAI> activeZombies;

    void Start() {
        spawnPoints = new List<Transform>();
        activeZombies = new List<ZombieAI>();

        // Find all spawn points in the scene with the "RespawnPoint" tag
        GameObject[] points = GameObject.FindGameObjectsWithTag("RespawnPoint");
        foreach (GameObject point in points) {
            spawnPoints.Add(point.transform);
        }

        if (spawnPoints.Count == 0) {
            Debug.LogError("No spawn points found! Please add objects with the 'RespawnPoint' tag.");
        }

        ZombieAI.OnZombieDeath += HandleZombieDeath;
        InvokeRepeating(nameof(CheckAndRespawnZombies), 0f, respawnInterval);
    }

    void OnDestroy() {
        ZombieAI.OnZombieDeath -= HandleZombieDeath;
    }

    void CheckAndRespawnZombies() {
        // Check if we need to spawn more zombies
        int zombiesToSpawn = minZombies - activeZombies.Count;

        if (zombiesToSpawn > 0 && activeZombies.Count < maxZombies) {
            for (int i = 0; i < zombiesToSpawn; i++) {
                if (activeZombies.Count >= maxZombies) break;  // Stop if we reach the max limit
                SpawnZombie();
            }
        }
    }

    void SpawnZombie() {
        // Select a random spawn point
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];

        Vector3 spawnPosition = spawnPoint.position + Random.insideUnitSphere * spawnRadius;
        spawnPosition.y = spawnPoint.position.y;

        GameObject zombieObject = Instantiate(zombiePrefab, spawnPosition, Quaternion.identity);
        ZombieAI zombie = zombieObject.GetComponent<ZombieAI>();
        if (zombie != null) {
            activeZombies.Add(zombie);
        } else {
            Debug.LogWarning("ZombiePrefab does not have a ZombieAI component.");
        }
    }

    void HandleZombieDeath(ZombieAI zombie) {
        if (activeZombies.Contains(zombie)) {
            activeZombies.Remove(zombie);
        }
    }
}

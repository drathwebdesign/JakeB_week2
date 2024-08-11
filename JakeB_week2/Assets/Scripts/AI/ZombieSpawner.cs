using System.Collections.Generic;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour {
    public GameObject zombiePrefab;
    public float spawnRadius = 10f;
    public int maxZombies = 8; // Max zombies per spawn point
    public int maxTotalZombies = 600; // Max zombies in the entire scene
    public float spawnInterval = 120f; // Set to a reasonable interval

    private List<Transform> respawnPoints;
    private List<ZombieAI> activeZombies;

    void Start() {
        respawnPoints = new List<Transform>();
        activeZombies = new List<ZombieAI>();

        GameObject[] points = GameObject.FindGameObjectsWithTag("RespawnPoint");
        foreach (GameObject point in points) {
            respawnPoints.Add(point.transform);
        }

        ZombieAI.OnZombieDeath += HandleZombieDeath;
        InvokeRepeating(nameof(SpawnZombies), 0f, spawnInterval);
    }

    void OnDestroy() {
        ZombieAI.OnZombieDeath -= HandleZombieDeath;
    }

    void SpawnZombies() {
        // don't exceed the max total zombies limit
        if (activeZombies.Count >= maxTotalZombies) {
            Debug.Log("Maximum total zombies reached: " + maxTotalZombies);
            return;
        }

        foreach (Transform respawnPoint in respawnPoints) {
            if (activeZombies.Count >= maxTotalZombies) {
                break;
            }

            if (CountActiveZombies(respawnPoint.position) < maxZombies) {
                Vector3 spawnPosition = respawnPoint.position + Random.insideUnitSphere * spawnRadius;
                spawnPosition.y = respawnPoint.position.y;

                GameObject zombieObject = Instantiate(zombiePrefab, spawnPosition, Quaternion.identity);
                ZombieAI zombie = zombieObject.GetComponent<ZombieAI>();
                if (zombie != null) {
                    activeZombies.Add(zombie);
                } else {
                    Debug.LogWarning("ZombiePrefab does not have a ZombieAI component.");
                }
            }
        }
    }

    int CountActiveZombies(Vector3 center) {
        Collider[] colliders = Physics.OverlapSphere(center, spawnRadius);
        int count = 0;
        foreach (Collider collider in colliders) {
            if (collider.CompareTag("Zombie")) {
                count++;
            }
        }
        return count;
    }

    void HandleZombieDeath(ZombieAI zombie) {
        if (activeZombies.Contains(zombie)) {
            activeZombies.Remove(zombie);
            Debug.Log("Zombie destroyed and removed: " + zombie.gameObject.name);
        }
    }
}

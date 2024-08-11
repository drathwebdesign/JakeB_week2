using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }
    private int score = 0;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    public void AddScore(int amount) {
        score += amount;
        Debug.Log("Score: " + score);

        // Update the UI using GameUIManager instead of UIManager
        GameUIManager uiManager = FindObjectOfType<GameUIManager>();
        if (uiManager != null) {
            uiManager.UpdateScoreUI();
        } else {
            Debug.LogWarning("GameUIManager not found in the scene.");
        }
    }

    public int GetScore() {
        return score;
    }
}

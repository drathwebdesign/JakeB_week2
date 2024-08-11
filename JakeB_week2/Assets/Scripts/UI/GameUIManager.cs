using UnityEngine;
using TMPro;

public class GameUIManager : MonoBehaviour {
    public TextMeshProUGUI scoreText;  // Use this for TextMeshPro

    private void Start() {
        UpdateScoreUI();  // Initialize the score display
    }

    public void UpdateScoreUI() {
        if (scoreText != null) {
            scoreText.text = "Ladies Saved: " + GameManager.Instance.GetScore();
        }
    }
}

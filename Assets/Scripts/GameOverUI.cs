using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GameOverUI : MonoBehaviour
{
    [Header("Game Over UI Elements")]
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI playTimeText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI hitsTakenText;
    [SerializeField] private TextMeshProUGUI dashCountText;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void ShowGameOver()
    {
        gameObject.SetActive(true);
        UpdateStatsDisplay();
    }

    private void UpdateStatsDisplay()
    {
        System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(PlayerStats.Instance.playTime);
        playTimeText.text = $"{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
        
        gameOverText.text = PlayerStats.Instance.isGameLoose ? "Game Over" : "You Win!";
        playerNameText.text = PlayerStats.Instance.playerName;
        scoreText.text = PlayerStats.Instance.score.ToString();
        hitsTakenText.text = PlayerStats.Instance.hitsTaken.ToString();
        dashCountText.text = PlayerStats.Instance.dashCount.ToString();
    }
}

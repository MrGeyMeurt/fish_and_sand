using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }
    public string playerName
    {
        get => PlayerPrefs.GetString("PlayerName", "Player");
        set => PlayerPrefs.SetString("PlayerName", value);
    }
    public float playTime;
    public int score;
    public int hitsTaken;
    public int dashCount;
    public bool isGameLoose = false;

    private bool isCountingTime = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        // DontDestroyOnLoad(gameObject);
        Debug.Log($"Nom du joueur chargé : {PlayerStats.Instance.playerName}");
    }

    private void Update()
    {
        if (isCountingTime && GameRule.Instance.IsGamePlaying())
        {
            playTime += Time.deltaTime;
        }
    }

    public void SavePlayerName(string newName)
    {
        playerName = newName;
        PlayerPrefs.Save();
    }

    public void AddScore(int amount)
    {
        if (GameRule.Instance != null && !GameRule.Instance.IsGameOver())
        {
            score += amount;
        }
    }

    public void AddHit()
    {
        if (GameRule.Instance != null && !GameRule.Instance.IsGameOver())
        {
            hitsTaken++;
        }
    }

    public void AddDash()
    {
        if (GameRule.Instance != null && !GameRule.Instance.IsGameOver())
        {
            dashCount++;
        }
    }

    public void SaveStats()
    {
        PlayerPrefs.SetFloat("PlayTime", playTime);
        PlayerPrefs.SetInt("Score", score);
        PlayerPrefs.SetInt("HitsTaken", hitsTaken);
        PlayerPrefs.SetInt("DashCount", dashCount);
        PlayerPrefs.Save();
    }

    public void LoadStats()
    {
        playTime = PlayerPrefs.GetFloat("PlayTime", 0f);
        score = PlayerPrefs.GetInt("Score", 0);
        hitsTaken = PlayerPrefs.GetInt("HitsTaken", 0);
        dashCount = PlayerPrefs.GetInt("DashCount", 0);
    }

    public void GameOver()
    {
        isGameLoose = true;
        SaveStats();
    }

    public void StopCountingTime() => isCountingTime = false;
}
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    public string playerName = "Player";
    public float playTime;
    public int score;
    public int hitsTaken;
    public int dashCount;

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
    }

    private void Update()
    {
        if (isCountingTime && GameRule.Instance.IsGamePlaying())
        {
            playTime += Time.deltaTime;
        }
    }

    public void AddScore(int amount) => score += amount;
    public void AddHit() => hitsTaken++;
    public void AddDash() => dashCount++;
    public void StopCountingTime() => isCountingTime = false;
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShowPlayerStats : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI hitsText;
    [SerializeField] private TextMeshProUGUI dashesText;
    [SerializeField] private TextMeshProUGUI resultText;

    private void Start()
    {
        RefreshStatsDisplay();
    }

    public void RefreshStatsDisplay()
    {
        if(PlayerStats.Instance == null)
        {
            Debug.Log("PlayerStats instance missing!");
            return;
        }

        PlayerStats.Instance.LoadStats();

        float playTime = PlayerStats.Instance.playTime;
        int score = PlayerStats.Instance.score;
        int hits = PlayerStats.Instance.hitsTaken;
        int dashes = PlayerStats.Instance.dashCount;

        timeText.text = $"Time: {playTime.ToString("F2")}s";
        scoreText.text = $"Score: {score}";
        hitsText.text = $"Hits Taken: {hits}";
        dashesText.text = $"Dashes Used: {dashes}";
    }

    public void ResetStats()
    {
        PlayerPrefs.DeleteAll();
        PlayerStats.Instance.LoadStats();
        RefreshStatsDisplay();
        Debug.Log("Stats reset!");
    }
}
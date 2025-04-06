using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GameStartCountdownUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countdownText;

    private void Start()
    {
        GameRule.Instance.OnStateChanged += GameRule_OnStateChanged;

        Hide();
    }

    private void GameRule_OnStateChanged(object sender, System.EventArgs e)
    {
        if (GameRule.Instance.IsCountdownToStartActive())
        {
            Show();
        } else
        {
            Hide();
        }
    }

    private void Update()
    {
        countdownText.text = Mathf.Ceil(GameRule.Instance.GetCountdownToStartTimer()).ToString();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}

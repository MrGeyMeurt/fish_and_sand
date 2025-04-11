using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

[System.Serializable]
public struct DeviceImageEntry
{
    public Image image;
    public Sprite playstationSprite;
    public Sprite keyboardSprite;
}

public class DeviceCheck : MonoBehaviour
{
    [SerializeField] private DeviceImageEntry[] deviceImages;
    
    private bool lastInputWasGamepad;
    private float gamepadVerifyThreshold = 0.2f;
    private float lastInputTime;
    private float inputCooldown = 0.5f;

    private void OnEnable()
    {
        InputSystem.onActionChange += HandleActionChange;
        if (GameRule.Instance != null)
        {
            GameRule.Instance.OnStateChanged += HandleGameStateChanged;
        }
        UpdateCursorState();
        UpdateDisplay();
    }

    private void OnDisable()
    {
        InputSystem.onActionChange -= HandleActionChange;
        if (GameRule.Instance != null)
        {
            GameRule.Instance.OnStateChanged -= HandleGameStateChanged;
        }
    }

    private void HandleGameStateChanged(object sender, EventArgs e)
    {
        UpdateCursorState();
        UpdateDisplay();
    }

    private void HandleActionChange(object obj, InputActionChange change)
    {
        if (change != InputActionChange.ActionPerformed) return;
        if (Time.time - lastInputTime < inputCooldown) return;

        var action = obj as InputAction;
        if (action == null) return;

        var control = action.activeControl;
        if (control == null) return;

        // Use generic input validation
        if (control.device is Gamepad)
        {
            // Check if input is significant using deadzone
            if (!control.IsActuated(gamepadVerifyThreshold))
                return;
        }

        lastInputWasGamepad = control.device is Gamepad;
        lastInputTime = Time.time;
        UpdateCursorState();
        UpdateDisplay();

        Debug.Log($"Input detected: {control.device.name} - {control.name}");
    }

    private void UpdateCursorState()
    {
        bool isMainMenu = SceneManager.GetActiveScene().name == "MainMenu";
        bool isGamePaused = GameRule.Instance != null && GameRule.Instance.IsGamePaused();

        if (isMainMenu)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            return; 
        }

        if (isGamePaused)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            return;
        }

        if (lastInputWasGamepad)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void UpdateDisplay()
    {
        foreach (var entry in deviceImages)
        {
            if (entry.image == null) continue;

            Sprite targetSprite = lastInputWasGamepad ? entry.playstationSprite : entry.keyboardSprite;
            entry.image.sprite = targetSprite;

            Color color = entry.image.color;
            color.a = targetSprite != null ? 1f : 0f;
            entry.image.color = color;
        }
    }
}
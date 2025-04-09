using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
        UpdateCursorState();
        UpdateDisplay();
    }

    private void OnDisable()
    {
        InputSystem.onActionChange -= HandleActionChange;
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
    }

    // Rest of the class remains the same
    private void UpdateCursorState()
    {
        bool isMainMenu = SceneManager.GetActiveScene().name == "MainMenu";
        bool isGamePaused = GameRule.Instance != null && GameRule.Instance.IsGamePaused();

        if (lastInputWasGamepad)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            if (isMainMenu && !lastInputWasGamepad || isGamePaused)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
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
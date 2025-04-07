using UnityEngine;
using UnityEngine.InputSystem;
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
    private bool hadAnyInput;

    private void Update()
    {
        bool gamepadInput = Gamepad.current?.wasUpdatedThisFrame ?? false;
        bool keyboardInput = CheckKeyboardMouseInput();

        if (gamepadInput)
        {
            lastInputWasGamepad = true;
            hadAnyInput = true;
        }
        else if (keyboardInput)
        {
            lastInputWasGamepad = false;
            hadAnyInput = true;
        }

        bool isMainMenu = SceneManager.GetActiveScene().name == "MainMenu";
        bool isGamePaused = GameRule.Instance != null && GameRule.Instance.IsGamePaused();

        if(!lastInputWasGamepad)
        {
            if (isMainMenu || isGamePaused)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = lastInputWasGamepad ? CursorLockMode.Locked : CursorLockMode.None;
        }

        if (hadAnyInput)
        {
            UpdateDisplay();
        }
    }

    private bool CheckKeyboardMouseInput()
    {
        if (Keyboard.current.anyKey.wasPressedThisFrame)
            return true;

        if (Mouse.current.delta.ReadValue() != Vector2.zero)
            return true;

        return false;
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
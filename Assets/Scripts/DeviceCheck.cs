using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DeviceCheck : MonoBehaviour
{
    [SerializeField] private Image Image;
    [SerializeField] private Sprite playstationSprite;
    [SerializeField] private Sprite keyboardSprite;

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
        Color color = Image.color;
            
        if (lastInputWasGamepad)
        {
            Image.sprite = playstationSprite;
            color.a = playstationSprite != null ? 1f : 0f;
        }
        else
        {
            Image.sprite = keyboardSprite;
            color.a = keyboardSprite != null ? 1f : 0f;
        }
        
        Image.color = color;
    }
}
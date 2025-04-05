using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject _defaultButton;
    [SerializeField] private EventSystem _eventSystem;

    void Update()
    {
        bool usingGamepad = Gamepad.current != null;

        if (usingGamepad)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (usingGamepad && _eventSystem != null && _eventSystem.currentSelectedGameObject == null)
        {
            _eventSystem.SetSelectedGameObject(_defaultButton);
        }
    }
}
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject _defaultButton;
    [SerializeField] private EventSystem _eventSystem;
    [SerializeField] private float _inputDeadzone = 0.1f;
    
    private bool _hadSelectionThisFrame;
    private float _lastInputTime;

    private void Update()
    {
        if (Gamepad.current == null || _eventSystem == null)
            return;

        _hadSelectionThisFrame = _eventSystem.currentSelectedGameObject != null;

        if (!_hadSelectionThisFrame && CheckNavigationInput())
        {
            SetDefaultSelection();
        }

        if (Time.time - _lastInputTime > 0.2f)
        {
            _hadSelectionThisFrame = false;
        }
    }

    private void SetDefaultSelection()
    {
        _eventSystem.SetSelectedGameObject(_defaultButton);
        _lastInputTime = Time.time;
    }

    private bool CheckNavigationInput()
    {
        Vector2 leftStick = ApplyDeadzone(Gamepad.current.leftStick.ReadValue());
        Vector2 dpad = ApplyDeadzone(Gamepad.current.dpad.ReadValue());

        bool hasInput = leftStick != Vector2.zero || dpad != Vector2.zero;
        
        return hasInput && !_hadSelectionThisFrame;
    }

    private Vector2 ApplyDeadzone(Vector2 input)
    {
        return input.magnitude < _inputDeadzone ? Vector2.zero : input.normalized;
    }
}
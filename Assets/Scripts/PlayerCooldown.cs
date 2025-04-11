using UnityEngine;
using UnityEngine.AI;
using StarterAssets;

public class PlayerCooldown : MonoBehaviour
{
    private ThirdPersonController _controller;
    private float _originalMoveSpeed;
    private float _originalSprintSpeed;
    private int _activeCooldownCount;
    private bool _isGameActive = true;

    private void Awake()
    {
        _controller = GetComponent<ThirdPersonController>();
        _originalMoveSpeed = _controller.MoveSpeed;
        _originalSprintSpeed = _controller.SprintSpeed;
    }

    public void StartCooldown()
    {
        _activeCooldownCount++;
        UpdateSpeed();
    }

    public void EndCooldown()
    {
        _activeCooldownCount = Mathf.Max(0, _activeCooldownCount - 1);
        UpdateSpeed();
    }

    public void SetGameActiveState(bool isActive)
    {
        _isGameActive = isActive;
        UpdateSpeed();
    }

    private void UpdateSpeed()
    {
        bool shouldStop = !_isGameActive || _activeCooldownCount > 0;
        
        _controller.MoveSpeed = shouldStop ? 0f : _originalMoveSpeed;
        _controller.SprintSpeed = shouldStop ? 0f : _originalSprintSpeed;
        
        Debug.Log($"Game Active: {_isGameActive} | Cooldowns: {_activeCooldownCount} | Speed: {_controller.MoveSpeed}");
    }
}
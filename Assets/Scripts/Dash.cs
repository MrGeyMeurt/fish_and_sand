using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;

public class DashController : MonoBehaviour
{
    [Header("Dash Settings")]
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = .5f;
    [SerializeField] private TrailRenderer dashTrail;

    private CharacterController _controller;
    private List<Gamepad> _activeGamepads = new List<Gamepad>();
    private StarterAssetsInputs _input;
    private bool _canDash = true;
    private Transform _mainCamera;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<StarterAssetsInputs>();
        _mainCamera = Camera.main.transform;

        if (dashTrail != null) dashTrail.emitting = false;
    }

    private void Update()
    {
        if (_input.dash && _canDash)
        {
            _input.dash = false;
            StartCoroutine(PerformDash());
        }
    }

    public void SetCanDash(bool canDash)
    {
        _canDash = canDash;
    }

    private IEnumerator PerformDash()
    {
        _canDash = false;

        foreach(Gamepad gamepad in Gamepad.all)
        {
            try 
            {
                gamepad.SetMotorSpeeds(0.2f, 0.2f);
                StartCoroutine(StopVibration(gamepad));
            }
            catch 
            {
            }
        }

        // Calculate direction from camera orientation
        Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;
        Vector3 dashDirection = inputDirection.magnitude == 0 ? 
            _mainCamera.forward : 
            _mainCamera.forward * inputDirection.z + _mainCamera.right * inputDirection.x;

        dashDirection.y = 0;
        dashDirection.Normalize();

        float timer = 0f;
        Vector3 dashVelocity = dashDirection * (dashDistance / dashDuration);

        if(dashTrail != null) dashTrail.emitting = true;

        // Dash movement
        while(timer < dashDuration)
        {
            _controller.Move(dashVelocity * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        if(dashTrail != null) dashTrail.emitting = false;
        yield return new WaitForSeconds(dashCooldown);
        
        _canDash = true;
        _input.dash = false;
    }

    private IEnumerator StopVibration(Gamepad targetGamepad)
    {
        yield return new WaitForSecondsRealtime(0.2f);
        
        try
        {
            if(targetGamepad != null)
                targetGamepad.ResetHaptics();
        }
        catch {}
    }

    private void OnDisable()
    {
        foreach(Gamepad gamepad in _activeGamepads)
        {
            try
            {
                gamepad?.ResetHaptics();
            }
            catch {}
        }
        _activeGamepads.Clear();
    }
}
using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;

public class DashController : MonoBehaviour
{
    [Header("Dash Settings")]
    [SerializeField] private float dashDistance = 10f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 6f;
    [SerializeField] private TrailRenderer dashTrail;
    [SerializeField] private int maxDashes = 2;

    [Header("UI Elements")]
    [SerializeField] private GameObject dash1Active;
    [SerializeField] private GameObject dash1Disabled;
    [SerializeField] private GameObject dash2Active;
    [SerializeField] private GameObject dash2Disabled;

    private CharacterController _controller;
    private StarterAssetsInputs _input;
    private Transform _mainCamera;
    private Queue<float> _rechargeTimes = new Queue<float>();
    private int currentDashes;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<StarterAssetsInputs>();
        _mainCamera = Camera.main.transform;
        
        currentDashes = maxDashes;
        UpdateDashUI();
        if (dashTrail != null) dashTrail.emitting = false;
    }

    private void Update()
    {
        if (GameRule.Instance == null || !GameRule.Instance.IsGamePlaying()) return;

        // Reload dashes
        while(_rechargeTimes.Count > 0 && Time.time >= _rechargeTimes.Peek())
        {
            _rechargeTimes.Dequeue();
            currentDashes = Mathf.Min(currentDashes + 1, maxDashes);
            UpdateDashUI();
        }

        if (_input.dash && currentDashes > 0)
        {
            _input.dash = false;
            StartCoroutine(PerformDash());
        }
        else if (_input.dash)
        {
            _input.dash = false;
        }
    }

    private IEnumerator PerformDash()
    {
        currentDashes--;
        _rechargeTimes.Enqueue(Time.time + dashCooldown);
        UpdateDashUI();
        PlayerStats.Instance.AddDash();

        // Vibration
        foreach(Gamepad gamepad in Gamepad.all)
        {
            try 
            {
                gamepad.SetMotorSpeeds(0.2f, 0.2f);
                StartCoroutine(StopVibration(gamepad));
            }
            catch { }
        }

        // Direction calculation
        Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;
        Vector3 dashDirection = inputDirection.magnitude == 0 ? 
            _mainCamera.forward : 
            _mainCamera.forward * inputDirection.z + _mainCamera.right * inputDirection.x;

        dashDirection.y = 0;
        dashDirection.Normalize();

        // Move
        float timer = 0f;
        Vector3 dashVelocity = dashDirection * (dashDistance / dashDuration);

        if(dashTrail != null) dashTrail.emitting = true;

        while(timer < dashDuration)
        {
            _controller.Move(dashVelocity * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        if(dashTrail != null) dashTrail.emitting = false;
    }

    private void UpdateDashUI()
    {
        dash1Active.SetActive(currentDashes >= 1);
        dash1Disabled.SetActive(currentDashes < 1);
        dash2Active.SetActive(currentDashes >= 2);
        dash2Disabled.SetActive(currentDashes < 2);
    }

    public void ResetDashes()
    {
        currentDashes = 0;
        _rechargeTimes.Clear();
        _rechargeTimes.Enqueue(Time.time + dashCooldown);
        _rechargeTimes.Enqueue(Time.time + dashCooldown * 2);
        UpdateDashUI();
    }

    private IEnumerator StopVibration(Gamepad targetGamepad)
    {
        yield return new WaitForSecondsRealtime(0.2f);
        try { targetGamepad?.ResetHaptics(); } catch { }
    }

    private void OnDisable()
    {
        foreach(Gamepad gamepad in Gamepad.all)
        {
            try { gamepad?.ResetHaptics(); } catch { }
        }
    }
}
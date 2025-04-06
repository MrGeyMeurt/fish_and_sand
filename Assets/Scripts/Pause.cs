using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;
using UnityEngine.InputSystem;

public class Pause : MonoBehaviour
{
    private StarterAssetsInputs _input;
    private CharacterController _controller;
    [SerializeField] private GameRule gameRule;

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<StarterAssetsInputs>();
    }

    void Update()
    {
        if (_input.pause)
        {
            _input.pause = false;
            gameRule?.GamePause();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}

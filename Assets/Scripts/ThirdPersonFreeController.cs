using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonFreeController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        [SerializeField]
        private float VerticalSpeed = 5.0f;
        [Tooltip("Up and down speed of the character in m/s")]
        [SerializeField]
        private float HorizontalSpeed = 5.0f;

        private CharacterController _controller;
        private StarterAssetsInputs _input;

        private void Start()
        {
            // get the character controller ( capsule collider )
            _controller = GetComponent<CharacterController>(); //physic of the character
            _input = GetComponent<StarterAssetsInputs>(); //input
            Debug.Log($"_input: {_input}");
            Debug.Log($"_controller: {_controller}");

        }

        private void Update()
        {
            Move(); // use the move method when the game is running
        }

        private void Move()
        {
            Vector3 verticalDirection = _input.move.z * transform.up * VerticalSpeed * Time.deltaTime;
            Vector3 horizontalDirection = new Vector3(_input.move.x, 0.0f, _input.move.y) * HorizontalSpeed * Time.deltaTime;

            _controller.Move(verticalDirection + horizontalDirection);

        }
    }
}
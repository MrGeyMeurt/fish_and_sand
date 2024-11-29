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
        private GameObject _mainCamera;

        private void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();

        }

        private void Update()
        {
            Move();
        }

        private void Move()
        {
            Vector3 verticalDirection = _input.move.z * transform.up * VerticalSpeed * Time.deltaTime;
            Vector3 horizontalDirection = new Vector3(_input.move.x, 0.0f, _input.move.y) * HorizontalSpeed * Time.deltaTime;

            _controller.Move(verticalDirection + horizontalDirection);

        }
    }
}
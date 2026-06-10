using UnityEngine;
using UnityEngine.InputSystem;
using PeopleFlow.Gameplay;

namespace PeopleFlow.Core
{
    /// <summary>
    /// Reads pointer/touch input via the New Input System and triggers MinionQueue logic.
    /// </summary>
    public class InputReader : MonoBehaviour
    {
        [SerializeField] private Camera raycastCamera;
        [SerializeField] private LayerMask queueMask = ~0;
        [SerializeField] private float holdRepeatInterval = 0.18f;
        [SerializeField] private float maxRayDistance = 200f;

        private MinionQueue _heldQueue;
        private float _repeatTimer;
        private bool _wasPressed;
        private bool _isInputEnabled = true;

        public void SetEnabled(bool enabled)
        {
            _isInputEnabled = enabled;
            if (!enabled) ReleaseHold();
        }

        private void Awake()
        {
            if (raycastCamera == null) raycastCamera = Camera.main;
        }

        private void Update()
        {
            if (!_isInputEnabled) return;

            bool isPressed = IsPointerPressed(out Vector2 screenPosition);

            if (isPressed && !_wasPressed)
            {
                // Press began: find queue and push immediately
                _heldQueue = RaycastQueue(screenPosition);
                if (_heldQueue != null)
                {
                    bool canPush = _heldQueue.TryPushRow();
                    // Debug.Log("Can push: " + canPush);
                    _repeatTimer = holdRepeatInterval;
                }
            }
            else if (isPressed && _wasPressed && _heldQueue != null)
            {
                // Holding: repeat push
                _repeatTimer -= Time.deltaTime;
                if (_repeatTimer <= 0f)
                {
                    bool canHoldPus = _heldQueue.TryPushRow();
                    // Debug.Log("Can hold push: " + canHoldPus);
                    _repeatTimer = holdRepeatInterval;
                }
            }
            else if (!isPressed && _wasPressed)
            {
                ReleaseHold();
            }

            _wasPressed = isPressed;
        }

        private void ReleaseHold()
        {
            _heldQueue = null;
            _repeatTimer = 0f;
        }

        private bool IsPointerPressed(out Vector2 screenPosition)
        {
            screenPosition = default;
            var touch = Touchscreen.current;
            if (touch != null && touch.primaryTouch.press.isPressed)
            {
                screenPosition = touch.primaryTouch.position.ReadValue();
                return true;
            }
            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.isPressed)
            {
                screenPosition = mouse.position.ReadValue();
                return true;
            }
            return false;
        }

        private MinionQueue RaycastQueue(Vector2 screenPosition)
        {
            if (raycastCamera == null) return null;
            Ray ray = raycastCamera.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, queueMask))
                return hit.collider.GetComponentInParent<MinionQueue>();
            return null;
        }

        public void SetCamera(Camera cam) => raycastCamera = cam;
    }
}

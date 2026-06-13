using UnityEngine;

namespace PeopleFlow.Gameplay
{
    /// <summary>
    /// Visual indicator placed at the start of a conveyor path.
    /// </summary>
    public class ConveyorIndicator : MonoBehaviour
    {
        private Conveyor _conveyor;
        private Vector3 _offset = new Vector3(0, 0.05f, 0); // Slightly above ground to avoid z-fighting

        public void Initialize(Conveyor conveyor)
        {
            _conveyor = conveyor;
            UpdatePosition();
        }

        // private void LateUpdate()
        // {
        //     if (_conveyor != null)
        //     {
        //         UpdatePosition();
        //     }
        // }

        private void UpdatePosition()
        {
            if (_conveyor == null || _conveyor.Path == null) return;

            // Place at the very beginning of the spline
            transform.position = _conveyor.Path.EvaluatePosition(0) + _offset;
            
            // Align with the path direction and lay flat on the floor
            Vector3 direction = _conveyor.Path.EvaluateDirection(0);
            if (direction.sqrMagnitude > 0.001f)
            {
                // Quaternion.LookRotation(direction, Vector3.up) makes it stand up.
                // To lay it flat, we look 'down' with the 'up' vector being the path direction
                transform.rotation = Quaternion.LookRotation(Vector3.down, direction);
            }
        }
    }
}
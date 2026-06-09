using UnityEngine;

namespace PeopleFlow.Gameplay
{
    /// <summary>Rotates this transform to always face the main camera (for world-space labels).</summary>
    [ExecuteAlways]
    public class Billboard : MonoBehaviour
    {
        private Camera _cam;

        private void LateUpdate()
        {
            if (_cam == null) _cam = Camera.main;
            if (_cam == null) return;
            transform.rotation = _cam.transform.rotation;
        }
    }
}

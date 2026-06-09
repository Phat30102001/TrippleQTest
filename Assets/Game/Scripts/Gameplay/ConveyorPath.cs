using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

namespace PeopleFlow.Gameplay
{
    /// <summary>
    /// Wraps a SplineContainer to provide distance-based movement logic for minions.
    /// </summary>
    [RequireComponent(typeof(SplineContainer))]
    public class ConveyorPath : MonoBehaviour
    {
        [SerializeField] private SplineContainer splineContainer;

        private float _totalPathLength;

        public float PathLength => _totalPathLength;

        public bool IsClosed => splineContainer != null && splineContainer.Spline != null && splineContainer.Spline.Closed;

        private void Awake()
        {
            if (splineContainer == null) splineContainer = GetComponent<SplineContainer>();
            RecalculateLength();
        }

        public void RecalculateLength()
        {
            if (splineContainer == null) splineContainer = GetComponent<SplineContainer>();
            _totalPathLength = splineContainer != null ? splineContainer.CalculateLength() : 0f;
            if (_totalPathLength < 0.0001f) _totalPathLength = 0.0001f;
        }

        /// <summary>
        /// Returns the normalized parameter [0, 1] for a given distance.
        /// Loops if the spline is closed, otherwise clamps or allows overshoot for logic.
        /// </summary>
        public float GetNormalizedParameter(float distance)
        {
            if (IsClosed)
            {
                float t = (distance % _totalPathLength) / _totalPathLength;
                if (t < 0f) t += 1f;
                return t;
            }
            else
            {
                return Mathf.Clamp01(distance / _totalPathLength);
            }
        }

        public Vector3 EvaluatePosition(float distance)
        {
            float t = GetNormalizedParameter(distance);
            return splineContainer.EvaluatePosition(t);
        }

        public Vector3 EvaluateDirection(float distance)
        {
            float t = GetNormalizedParameter(distance);
            float3 tangent = splineContainer.EvaluateTangent(t);
            Vector3 direction = ((Vector3)tangent).normalized;
            return direction.sqrMagnitude < 0.0001f ? transform.forward : direction;
        }

        public void SetSplineContainer(SplineContainer container)
        {
            splineContainer = container;
            RecalculateLength();
        }
    }
}

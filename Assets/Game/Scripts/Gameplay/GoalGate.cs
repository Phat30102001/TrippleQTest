using System;
using UnityEngine;
using PeopleFlow.Data;

namespace PeopleFlow.Gameplay
{
    /// <summary>
    /// Represents a single goal slot (hole) that consumes matching-colored minions.
    /// Uses swept point-segment distance detection to prevent tunneling at high speeds.
    /// </summary>
    public class GoalGate : MonoBehaviour
    {
        [SerializeField] private float captureRadius = 1.0f;
        [SerializeField] private Transform holeAnchor;

        [SerializeField] private GoalGateVisual visual;
        public GoalGateVisual Visual => visual;
        public MinionColor GoalColor { get; private set; }
        public int RemainingCount { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsCompleted { get; private set; }

        public event Action<GoalGate> OnCountChanged;
        public event Action<GoalGate> OnCompleted;

        private Conveyor _conveyor;
        private ColorPalette _palette;

        public void Initialize(GoalGateData data, Conveyor conveyor, ColorPalette palette)
        {
            GoalColor = data.color;
            RemainingCount = Mathf.Max(1, data.count);
            _conveyor = conveyor;
            _palette = palette;
            IsActive = false;
            IsCompleted = false;
        }

        public void Activate()
        {
            if (IsCompleted) return;
            IsActive = true;
            OnCountChanged?.Invoke(this); // Trigger visual update
        }

        private void Update()
        {
            // Debug.Log("isActive: " + IsActive + " isCompleted: " + IsCompleted + "");
            if (!IsActive || IsCompleted || _conveyor == null) return;

            Vector3 anchor = holeAnchor != null ? holeAnchor.position : transform.position;
            float radiusSquared = captureRadius * captureRadius;
            var minions = _conveyor.ActiveMinions;

            for (int i = 0; i < minions.Count; i++)
            {
                var minion = minions[i];
                if (minion == null || minion.IsLeaving) continue;
                if (minion.Color != GoalColor) continue;

                // Swept distance test on XZ plane between anchor and the minion's movement segment
                float distSq = GetSqrDistancePointSegmentXZ(anchor, minion.PreviousPosition, minion.transform.position);
                if (distSq <= radiusSquared)
                {
                    ConsumeMinion(minion, anchor);
                    if (IsCompleted) break;
                }
            }
        }

        private void ConsumeMinion(MinionAgent minion, Vector3 anchor)
        {
            minion.EnterGate(anchor);
            RemainingCount--;
            OnCountChanged?.Invoke(this);

            if (RemainingCount <= 0)
            {
                RemainingCount = 0;
                Complete();
            }
        }

        private void Complete()
        {
            if (IsCompleted) return;
            IsCompleted = true;
            IsActive = false;
            OnCompleted?.Invoke(this);
        }

        private static float GetSqrDistancePointSegmentXZ(Vector3 point, Vector3 a, Vector3 b)
        {
            Vector2 p = new Vector2(point.x, point.z);
            Vector2 p1 = new Vector2(a.x, a.z);
            Vector2 p2 = new Vector2(b.x, b.z);

            Vector2 ab = p2 - p1;
            float abLenSq = ab.sqrMagnitude;
            if (abLenSq < 1e-6f) return (p - p1).sqrMagnitude;
            float t = Mathf.Clamp01(Vector2.Dot(p - p1, ab) / abLenSq);
            Vector2 projection = p1 + ab * t;
            return (p - projection).sqrMagnitude;
        }

        public Color GetDisplayColor() => _palette != null ? _palette.GetColor(GoalColor) : Color.white;

        private void OnDrawGizmosSelected()
        {
            Vector3 center = holeAnchor != null ? holeAnchor.position : transform.position;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(center, captureRadius);
            
            // Draw a flat circle to visualize the XZ capture logic
            Gizmos.color = new Color(0, 1, 1, 0.3f);
            int segments = 20;
            for (int i = 0; i < segments; i++)
            {
                float angle1 = (float)i / segments * Mathf.PI * 2;
                float angle2 = (float)(i + 1) / segments * Mathf.PI * 2;
                Vector3 p1 = center + new Vector3(Mathf.Cos(angle1) * captureRadius, 0, Mathf.Sin(angle1) * captureRadius);
                Vector3 p2 = center + new Vector3(Mathf.Cos(angle2) * captureRadius, 0, Mathf.Sin(angle2) * captureRadius);
                Gizmos.DrawLine(p1, p2);
            }
        }
    }
}

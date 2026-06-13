using System;
using System.Collections;
using PeopleFlow.Core;
using UnityEngine;
using PeopleFlow.Data;

namespace PeopleFlow.Gameplay
{
    /// <summary>
    /// Controls a single minion's movement along the conveyor path.
    /// </summary>
    [RequireComponent(typeof(MinionView))]
    public class MinionAgent : MonoBehaviour
    {
        public MinionColor Color { get; private set; }
        public bool IsLeaving { get; private set; }
        public bool IsFollowingRow { get; set; }
        public Vector3 PreviousPosition { get; private set; }
        public GameObject PrefabOrigin { get; set; }
        public MinionRowAgent RowParent { get; set; }

        public  Action<MinionAgent> OnRemoved;
        public  Action OnActive;

        private Conveyor _conveyor;
        private MinionView _view;
        private float _currentDistance;
        private float _moveSpeed;
        private bool _isEntering;

        public float Distance => _currentDistance;

        private void Awake()
        {
            _view = GetComponent<MinionView>();
        }

        public void SetData(Conveyor conveyor, float startDistance, float speed, MinionColor color,
            UnityEngine.Color displayColor)
        {
            _conveyor = conveyor;
            _currentDistance = startDistance;
            _moveSpeed = speed;
            Color = color;
            IsLeaving = false;

            if (_view == null) _view = GetComponent<MinionView>();
            _view.SetColor(displayColor);
            _view.SetRunning(speed > 0.001f && !_isEntering);

            if (!_isEntering && !IsFollowingRow)
            {
                UpdateTransform();
                PreviousPosition = transform.position;
            }
            gameObject.SetActive(true);
            // OnActive?.Invoke();
        }

        public void SetOnConveyor(Conveyor conveyor, float startDistance, float speed)
        {
            _conveyor = conveyor;
            _currentDistance = startDistance;
            _moveSpeed = speed;
            _isEntering = false;

            // Only snap to spline center if NOT in a managed row
            if (!IsFollowingRow)
            {
                UpdateTransform();
            }

            PreviousPosition = transform.position;
            if (_view != null) _view.SetRunning(speed > 0.001f);
        }

        public void AnimateEntry(Conveyor targetConveyor, float targetDistance, float speed,
            UnityEngine.Color displayColor)
        {
            StartCoroutine(PerformEntryJump(targetConveyor, targetDistance, speed, displayColor));
        }

        private IEnumerator PerformEntryJump(Conveyor targetConveyor, float targetDistance, float speed,
            UnityEngine.Color displayColor)
        {
            _isEntering = true;
            _conveyor = targetConveyor;

            Vector3 startPos = transform.position;
            float jumpDuration = 0.5f;
            float elapsed = 0f;

            if (_view == null) _view = GetComponent<MinionView>();
            _view.SetColor(displayColor);
            _view.SetRunning(false);
            _view.PlayJump();

            while (elapsed < jumpDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / jumpDuration;

                Vector3 endPos = _conveyor.Path.EvaluatePosition(targetDistance);
                Vector3 currentPos = Vector3.Lerp(startPos, endPos, t);
                currentPos.y += Mathf.Sin(t * Mathf.PI) * 1.5f;

                transform.position = currentPos;

                Vector3 dir = (endPos - startPos);
                dir.y = 0;
                if (dir.sqrMagnitude > 0.001f)
                    transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

                yield return null;
            }

            _isEntering = false;
            SetData(targetConveyor, targetDistance, speed, Color, displayColor);
            transform.SetParent(targetConveyor.transform);
        }

        private void Update()
        {
            if (IsLeaving || _isEntering || IsFollowingRow || _conveyor == null || _moveSpeed < 0.001f) return;

            PreviousPosition = transform.position;
            _currentDistance += _moveSpeed * Time.deltaTime;

            if (!_conveyor.Path.IsClosed && _currentDistance >= _conveyor.Path.PathLength)
            {
                if (_conveyor.NextConveyor != null)
                {
                    float overshoot = _currentDistance - _conveyor.Path.PathLength;
                    SwitchConveyor(_conveyor.NextConveyor, overshoot);
                }
                else
                {
                    _currentDistance = _conveyor.Path.PathLength;
                }
            }

            UpdateTransform();
        }

        private void SwitchConveyor(Conveyor next, float startDistance)
        {
            _conveyor.RemoveAgent(this);
            _conveyor = next;
            // No direct AddAgent on Conveyor anymore, it handles via rows or direct
            _currentDistance = startDistance;
            transform.SetParent(_conveyor.transform);
        }

        private void UpdateTransform()
        {
            if (_conveyor == null || _conveyor.Path == null) return;

            transform.position = _conveyor.Path.EvaluatePosition(_currentDistance);
            Vector3 direction = _conveyor.Path.EvaluateDirection(_currentDistance);
            if (direction.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        public void EnterGate(Vector3 targetHolePosition)
        {
            if (IsLeaving) return;

            IsLeaving = true;
            IsFollowingRow = false;
            // transform.SetParent(null);

            _view.PlayJump();
            StartCoroutine(PerformJumpToHole(targetHolePosition));
        }

        private IEnumerator PerformJumpToHole(Vector3 target)
        {
            Vector3 startPos = transform.position;
            Vector3 peakOffset = Vector3.up * 0.6f;
            const float jumpDuration = 0.45f;
            float timer = 0f;

            while (timer < jumpDuration)
            {
                timer += Time.deltaTime;
                float progress = Mathf.Clamp01(timer / jumpDuration);
                Vector3 basePosition = Vector3.Lerp(startPos, target, progress);
                float height = Mathf.Sin(progress * Mathf.PI);

                transform.position = basePosition + peakOffset * height;

                Vector3 lookDirection = (target - transform.position);
                lookDirection.y = 0f;
                if (lookDirection.sqrMagnitude > 0.0001f)
                    transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);

                yield return null;
            }

            OnRemoved?.Invoke(this);
            //
            // if (PrefabOrigin != null && PoolManager.Instance != null)
            //     PoolManager.Instance.Return(PrefabOrigin, gameObject);
            // else
            // Destroy(gameObject);
            gameObject.SetActive(false);
        }


        public void PlayJumpAnimation() => _view.PlayJump();

        public UnityEngine.Color GetDisplayColor() =>
            _view != null ? UnityEngine.Color.white : UnityEngine.Color.white; // Simplified
    }
}

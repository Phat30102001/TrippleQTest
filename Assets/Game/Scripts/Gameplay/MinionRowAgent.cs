using System;
using System.Collections.Generic;
using UnityEngine;
using PeopleFlow.Data;
using PeopleFlow.Core;

namespace PeopleFlow.Gameplay
{
    /// <summary>
    /// Parent object for a row of minions. Handles movement along the conveyor as a group.
    /// </summary>
    public class MinionRowAgent : MonoBehaviour
    {
        private Conveyor _conveyor;
        private float _currentDistance;
        private float _moveSpeed;
        private bool _isEntering;
        
        public MinionColor RowColor { get; private set; }
        private readonly List<MinionAgent> _minions = new List<MinionAgent>();
        public IReadOnlyList<MinionAgent> Minions => _minions;

        public float Distance => _currentDistance;
        public bool IsEntering => _isEntering;
        public GameObject RowPrefabOrigin { get; set; }
        private bool _isPaused=false;
        private int activeMinionsCount=0;
        public void SetData(MinionColor color)
        {
            RowColor = color;
            _minions.Clear();
            _isEntering = false;
            _conveyor = null;
            gameObject.SetActive(true);
        }

        private void OnEnable()
        {
            EventBus.OnPauseGame += HandlePauseGame;
        }

        private void OnDisable()
        {
            EventBus.OnPauseGame -= HandlePauseGame;
        }

        private void HandlePauseGame(bool isPaused)
        {
            _isPaused = isPaused;
        }

        public void AddMinion(MinionAgent agent)
        {
            activeMinionsCount++;
            _minions.Add(agent);
            agent.transform.SetParent(transform);
            agent.IsFollowingRow = true;
            agent.OnRemoved += HandleMinionRemoved;
        }
        
        private void HandleMinionRemoved(MinionAgent agent)
        {
            // _minions.Remove(agent);
            agent.OnRemoved -= HandleMinionRemoved;
            activeMinionsCount--;
            if (activeMinionsCount <= 0)
            {
                // All minions in this row are gone
                if (_conveyor != null) _conveyor.RemoveRow(this);
                
                if (RowPrefabOrigin != null && PoolManager.Instance != null)
                {
                    // PoolManager.Instance.Return(RowPrefabOrigin, gameObject);
                    PoolManager.Instance.Deposit(this);
                }
                // else
                // {
                    gameObject.SetActive(false); 
                // }
            }
        }

        public void SetOnConveyor(Conveyor conveyor, float startDistance, float speed)
        {
            _conveyor = conveyor;
            _currentDistance = startDistance;
            _moveSpeed = speed;
            
            foreach (var m in _minions)
            {
                // Ensure child knows it should follow parent transform
                m.IsFollowingRow = true;
                m.SetOnConveyor(conveyor, startDistance, speed);
            }
            
            UpdateTransform();
        }

        public void AnimateEntry(Conveyor targetConveyor, float targetDistance, float speed)
        {
            StartCoroutine(PerformEntryJump(targetConveyor, targetDistance, speed));
        }

        private System.Collections.IEnumerator PerformEntryJump(Conveyor targetConveyor, float targetDistance, float speed)
        {
            _isEntering = true;
            _conveyor = targetConveyor;
            
            Vector3 startPos = transform.position;
            float jumpDuration = 0.5f;
            float elapsed = 0f;

            foreach (var m in _minions)
            {
                var view = m.GetComponent<MinionView>();
                if (view != null) view.PlayJump();
            }

            while (elapsed < jumpDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / jumpDuration;
                
                Vector3 endPos = targetConveyor.Path.EvaluatePosition(targetDistance);
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
            SetOnConveyor(targetConveyor, targetDistance, speed);
            transform.SetParent(targetConveyor.transform);
        }

        private void Update()
        {
            Debug.Log($"Update {_isPaused}" );
            if (_isPaused) return;
            if (_isEntering || _conveyor == null || _moveSpeed < 0.001f) return;
            
            _currentDistance += _moveSpeed * Time.deltaTime;
            
            // Check for path transition
            if (!_conveyor.Path.IsClosed && _currentDistance >= _conveyor.Path.PathLength)
            {
                if (_conveyor.NextConveyor != null)
                {
                    float overshoot = _currentDistance - _conveyor.Path.PathLength;
                    _conveyor.RemoveRow(this);
                    _conveyor = _conveyor.NextConveyor;
                    _conveyor.AddRowDirectly(this);
                    _currentDistance = overshoot;
                    transform.SetParent(_conveyor.transform);
                }
                else
                {
                    _currentDistance = _conveyor.Path.PathLength;
                }
            }

            UpdateTransform();
        }

        private void UpdateTransform()
        {
            if (_conveyor == null || _conveyor.Path == null) return;
            
            transform.position = _conveyor.Path.EvaluatePosition(_isPaused||activeMinionsCount<=0?0: _currentDistance);
            Vector3 direction = _conveyor.Path.EvaluateDirection(_isPaused||activeMinionsCount<=0?0: _currentDistance);
            if (direction.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
    }
}
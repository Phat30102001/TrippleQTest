using System;
using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;
using PeopleFlow.Data;
using PeopleFlow.Gameplay;

namespace PeopleFlow.Core
{
    /// <summary>
    /// Builds the game level at runtime using a LevelConfig asset.
    /// Spawns conveyor paths, minion queues, and goal lines.
    /// </summary>
    public class LevelLoader : MonoBehaviour
    {
        [Header("Level Controller")] private LevelConfig activeLevel;
        public LevelConfig GetActiveLevel() => activeLevel;
        [SerializeField] private ColorPalette palette;
        private LevelController levelController;
        [SerializeField] private GoalWinCondition winCondition;
        [SerializeField] private TimerSystem timerSystem;
        [SerializeField] private InputReader inputReader;

        [Header("Gameplay Prefabs")] [SerializeField]
        private GameObject minionPrefab;

        [SerializeField] private GameObject goalGatePrefab;
        [SerializeField] private MinionQueue minionQueuePrefab;
        [SerializeField] private GameObject goalContainerPrefab;
        [SerializeField] private GameObject clearBarrierPrefab;

        [Header("Session Management")] [SerializeField]
        private GameManager gameManager;

        [Header("Hierarchy Roots")]
        // [SerializeField] private GameObject conveyorRoot;

        [SerializeField]
        private GameObject queueRoot;

        // [SerializeField] private GameObject goalRoot;
        [SerializeField] private GameObject lockedGoalRoot;

        [Header("Minion Queue")] [SerializeField]
        private List<MinionQueue> minionQueues = new List<MinionQueue>();

        // private readonly List<Conveyor> _conveyors = new List<Conveyor>();
        
        private LevelSO _cachedLevelData;
        private int _levelIndex = -1;

        public void SetData(int levelIndex, LevelSO levelData, Action callback)
        {
            if (_levelIndex != levelIndex)
            {
                // destroy old level
                if (levelController != null)
                {
                    Destroy(levelController.gameObject);
                }

                BuildLevel(levelData.LevelController);
                _levelIndex = levelIndex;
                _cachedLevelData = levelData;
                activeLevel = _cachedLevelData.LevelConfig;
            }

            // 1. Set data level controller and conveyors
            if (levelController != null)
            {
                levelController.SetData(activeLevel);
            }

            // 2. Build queues that depend on conveyors
            BuildMinionQueues();


            if (inputReader != null) inputReader.SetEnabled(true);
            if (winCondition != null) winCondition.SetData(activeLevel.GetTotalGoalCount());
            if (timerSystem != null) timerSystem.Begin(activeLevel.timeLimit);

            callback?.Invoke();

        }

        private void BuildLevel(LevelController levelMap)
        {
            var map = Instantiate(levelMap, Vector3.zero, Quaternion.identity, transform);
            levelController = map;
            levelController.Init(palette);
        }

        private void BuildMinionQueues()
        {
            Transform parent = queueRoot != null ? queueRoot.transform : transform;
            int count = activeLevel.startColumns.Count;

            if (count > minionQueues.Count)
            {
                // instantiate more
                int unspawnAmount = count - minionQueues.Count;
                for (int i = 0; i < unspawnAmount; i++)
                {
                    var queueGo = Instantiate(minionQueuePrefab, Vector3.zero, Quaternion.identity, parent);
                    minionQueues.Add(queueGo.GetComponent<MinionQueue>());
                }
            }

            float spacing = activeLevel.queueSpacing;
            float totalWidth = (count - 1) * spacing;
            float startX = -totalWidth * 0.5f;
            for (int i = 0; i < count; i++)
            {
                var data = activeLevel.startColumns[i];
                Vector3 autoPos = new Vector3(startX + i * spacing, 0, activeLevel.queueZOffset);
                minionQueues[i].transform.position = autoPos;
                minionQueues[i].gameObject.SetActive(true);
                var targetConveyor = levelController.GetConveyor(data.conveyorIndex);
                minionQueues[i].Initialize(data, targetConveyor, palette);

            }
        }
        
        public void ResetLevel(Action callback)
        {
            EndLevel();
            SetData(_levelIndex, _cachedLevelData, callback);
        }

        public void EndLevel()
        {
            if (timerSystem != null) timerSystem.Stop();
            if (inputReader != null) inputReader.SetEnabled(false);
            levelController.EndLevel();
            foreach (var queue in minionQueues)
            {
                queue.gameObject.SetActive(false);
            }
        }

    }
}

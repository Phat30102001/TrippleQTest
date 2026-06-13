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
        [Header("Level Controller")] 
        [SerializeField] private LevelConfig activeLevel;
        public LevelConfig GetActiveLevel() => activeLevel;
        [SerializeField] private ColorPalette palette;
        [SerializeField] private LevelController levelController;
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

        [SerializeField] private GameObject queueRoot;
        // [SerializeField] private GameObject goalRoot;
        [SerializeField] private GameObject lockedGoalRoot;
        [Header("Minion Queue")]
        [SerializeField] private List<MinionQueue> minionQueues = new List<MinionQueue>();

        // private readonly List<Conveyor> _conveyors = new List<Conveyor>();

        public LevelConfig ActiveLevel => activeLevel;
        private LevelSO _cachedLevelData;
        private int _levelIndex=-1;
        
        public void SetData(int levelIndex,LevelSO levelData,Action callback)
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
                levelController.SetData( activeLevel);
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
            var map= Instantiate(levelMap,Vector3.zero,Quaternion.identity, transform);
            levelController = map;
            levelController.Init(minionPrefab, palette);
        }

        // private void BuildConveyorSystems()
        // {
        //     _conveyors.Clear();
        //     for (int i = 0; i < activeLevel.conveyors.Count; i++)
        //     {
        //         var data = activeLevel.conveyors[i];
        //         var go = new GameObject($"Conveyor_System_{i}");
        //         go.transform.SetParent(conveyorRoot != null ? conveyorRoot.transform : transform);
        //
        //         var container = go.AddComponent<SplineContainer>();
        //         container.Spline = new Spline(data.path);
        //         container.Spline.Closed = data.isClosed;
        //
        //         var path = go.AddComponent<ConveyorPath>();
        //         path.SetSplineContainer(container);
        //
        //         var conveyor = go.AddComponent<Conveyor>();
        //         conveyor.Setup(data, palette, minionPrefab);
        //         _conveyors.Add(conveyor);
        //     }
        //
        //     // Establish links between conveyors
        //     for (int i = 0; i < activeLevel.conveyors.Count; i++)
        //     {
        //         int nextIndex = activeLevel.conveyors[i].nextConveyorIndex;
        //         if (nextIndex >= 0 && nextIndex < _conveyors.Count)
        //         {
        //             _conveyors[i].NextConveyor = _conveyors[nextIndex];
        //         }
        //     }
        // }

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

            // for (int i = 0; i < count; i++)
            // {
            //     var data = activeLevel.startColumns[i];
            //     Vector3 autoPos = new Vector3(startX + i * spacing, 0, activeLevel.queueZOffset);
            //
            //     
            //     var queueGo = Instantiate(minionQueuePrefab, autoPos, Quaternion.identity, parent);
            //     var queue = queueGo;
            //     var targetConveyor = levelController.GetConveyor(data.conveyorIndex);
            //     queue.Initialize(data, targetConveyor, minionPrefab, palette);
            //     minionQueues.Add(queue);
            // }
        }

        // private void BuildGoalLines()
        // {
        //     Transform parent = goalRoot != null ? goalRoot.transform : transform;
        //     foreach (var data in activeLevel.goalLines)
        //     {
        //         var container = Instantiate(goalContainerPrefab, data.position, Quaternion.Euler(data.eulerRotation),
        //             parent);
        //         container.name = "Goal_Line_Container";
        //
        //         var factory = container.AddComponent<GoalFactory>();
        //         // factory.GoalGatePrefab = goalGatePrefab;
        //
        //         var line = container.AddComponent<GoalLine>();
        //         line.SetBarrierPrefab(clearBarrierPrefab);
        //
        //         var targetConveyor = levelController.GetConveyor(data.conveyorIndex);
        //         line.Build(data.gates, factory, targetConveyor, palette, false);
        //     }
        // }

        // private void BuildLockedGoalLines()
        // {
        //     Transform parent = lockedGoalRoot != null ? lockedGoalRoot.transform : transform;
        //     foreach (var data in activeLevel.lockedGoalLines)
        //     {
        //         var container = Instantiate(goalContainerPrefab, data.position, Quaternion.Euler(data.eulerRotation),
        //             parent);
        //         container.name = "Locked_Goal_Line_Container";
        //
        //         var factory = container.AddComponent<GoalFactory>();
        //         // factory.GoalGatePrefab = goalGatePrefab;
        //
        //         var line = container.AddComponent<GoalLine>();
        //         line.SetBarrierPrefab(clearBarrierPrefab);
        //
        //         var targetConveyor = levelController.GetConveyor(data.conveyorIndex);
        //         line.Build(data.gates, factory, targetConveyor, palette, true);
        //
        //         var controller = container.AddComponent<LockedGoalController>();
        //         controller.Initialize(data.unlockCounter);
        //     }
        // }
        public void ResetLevel(Action callback)
        {
            EndLevel();
            SetData(_levelIndex,_cachedLevelData,callback);
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

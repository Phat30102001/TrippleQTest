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
        [Header("Level Controller")] [SerializeField]
        private LevelConfig activeLevel;

        [SerializeField] private ColorPalette palette;
        [SerializeField] private LevelController levelController;

        [Header("Gameplay Prefabs")] [SerializeField]
        private GameObject minionPrefab;

        [SerializeField] private GameObject goalGatePrefab;
        [SerializeField] private GameObject minionQueuePrefab;
        [SerializeField] private GameObject goalContainerPrefab;
        [SerializeField] private GameObject clearBarrierPrefab;

        [Header("Session Management")] [SerializeField]
        private GameManager gameManager;

        [Header("Hierarchy Roots")] 
        // [SerializeField] private GameObject conveyorRoot;

        [SerializeField] private GameObject queueRoot;
        [SerializeField] private GameObject goalRoot;
        [SerializeField] private GameObject lockedGoalRoot;

        // private readonly List<Conveyor> _conveyors = new List<Conveyor>();

        public LevelConfig ActiveLevel => activeLevel;

        private void Start()
        {
            EventBus.Reset();

            // BuildConveyorSystems();
            BuildMinionQueues();
            // BuildGoalLines();
            // BuildLockedGoalLines();

            //Todo: load level prefab from level config, use Addressables to load level prefab
            levelController.Init(minionPrefab, palette,activeLevel);
            if (gameManager != null)
            {
                gameManager.StartSession(activeLevel.timeLimit, activeLevel.GetTotalGoalCount());
            }
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
            if (count == 0) return;

            float spacing = activeLevel.queueSpacing;
            float totalWidth = (count - 1) * spacing;
            float startX = -totalWidth * 0.5f;

            for (int i = 0; i < count; i++)
            {
                var data = activeLevel.startColumns[i];
                Vector3 autoPos = new Vector3(startX + i * spacing, 0, activeLevel.queueZOffset);

                var queueGo = Instantiate(minionQueuePrefab, autoPos, Quaternion.identity, parent);
                queueGo.name = $"Minion_Queue_{i}";

                var queue = queueGo.GetComponent<MinionQueue>();
                var targetConveyor = levelController.GetConveyor(data.conveyorIndex);
                queue.Initialize(data, targetConveyor, minionPrefab, palette);
            }
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


    }
}

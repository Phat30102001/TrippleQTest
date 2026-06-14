using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

namespace PeopleFlow.Data
{
    /// <summary>
    /// Data for a single goal slot (hole).
    /// </summary>
    [Serializable]
    public struct GoalGateData
    {
        public MinionColor color;
        [Min(1)] public int count;
    }

    /// <summary>
    /// An ordered sequence of goal gates in a line.
    /// </summary>
    [Serializable]
    public class GoalLineData
    {
        public Vector3 position;
        public Vector3 eulerRotation;
        public int conveyorIndex = 0;
        public List<GoalGateData> gates = new List<GoalGateData>();
    }

    /// <summary>
    /// A goal line that is initially locked (e.g., frozen in ice).
    /// </summary>
    [Serializable]
    public class LockedGoalLineData
    {
        public Vector3 position;
        public Vector3 eulerRotation;
        public int conveyorIndex = 0;
        [Min(1)] public int unlockCounter = 1;
        public List<GoalGateData> gates = new List<GoalGateData>();
    }

    /// <summary>
    /// Data for a group of minion rows of the same color.
    /// </summary>
    [Serializable]
    public struct MinionRowData
    {
        public MinionColor color;
        public int rowAmount;

        public MinionRowData(MinionColor color, int rowAmount)
        {
            this.color = color;
            this.rowAmount = rowAmount;
        }

    }

    /// <summary>
    /// Data for a column of minions waiting to be pushed.
    /// </summary>
    [Serializable]
    public class MinionQueueData
    {
        public int conveyorIndex = 0;
        /// <summary>
        /// Sequence of row groups.
        /// </summary>
        public List<MinionRowData> rows = new List<MinionRowData>();
    }

    /// <summary>
    /// Configuration for a single conveyor belt segment.
    /// </summary>
    [Serializable]
    public class ConveyorData
    {
        [Tooltip("Path the minions travel along.")]
        public Spline path = new Spline();
        public bool isClosed = true;
        [Tooltip("Index of the next conveyor in the sequence (-1 for none).")]
        public int nextConveyorIndex = -1;
        
        [Min(1)] public int capacityRows = 8;
        [Min(0.1f)] public float minionSpeed = 3f;
        [Min(0.1f)] public float rowSpacing = 1.5f;
        [Min(1)] public int minionsPerRow = 3;
    }

    /// <summary>
    /// Complete data-driven definition of a level.
    /// </summary>
    [CreateAssetMenu(menuName = "PeopleFlow/Level Config", fileName = "Level_000")]
    public class LevelConfig : ScriptableObject
    {
        [Min(1f)] public float timeLimit = 60f;
        [Tooltip("Horizontal spacing between minion columns.")]
        public float queueSpacing = 2f;
        [Tooltip("Common Z position for all minion columns.")]
        public float queueZOffset = -10f;

        [Header("Global Capacity")]
        [Tooltip("Total number of minion rows allowed on all conveyors combined.")]
        [Min(1)] public int maxGlobalCapacityRows = 10;

        public List<ConveyorData> conveyors = new List<ConveyorData>();
        public List<MinionQueueData> startColumns = new List<MinionQueueData>();
        public List<GoalLineData> goalLines = new List<GoalLineData>();
        public List<LockedGoalLineData> lockedGoalLines = new List<LockedGoalLineData>();

        /// <summary>
        /// Total number of goal gates in the level.
        /// </summary>
        public int GetTotalGoalCount()
        {
            int total = 0;
            foreach (var line in goalLines) total += line.gates.Count;
            foreach (var line in lockedGoalLines) total += line.gates.Count;
            return total;
        }
    }

    public static class LevelConfigExtensions
    {
        public static GoalLineData CreateDefaultGoalLineData()
        {
            Debug.Log("Creating default goal line data");
            return new GoalLineData()
            {
                position = Vector3.zero,
                eulerRotation = Vector3.zero,
                conveyorIndex = 0,
                gates = new List<GoalGateData>()
            };
        }
    }
}

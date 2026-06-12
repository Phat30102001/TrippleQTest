using System;
using System.Collections.Generic;
using PeopleFlow.Data;
using PeopleFlow.Core;
using UnityEngine;

namespace PeopleFlow.Gameplay
{

    public class LevelController : MonoBehaviour
    {
        public List<Conveyor> ConveyorList;
        public GoalFactory GoalFactory;

        private int _maxGlobalCapacity;
        private int _totalActiveRows;

        public bool IsGlobalCapacityFull => _totalActiveRows >= _maxGlobalCapacity;

        public void Init(GameObject minionPrefab, ColorPalette palette, LevelConfig activeLevel)
        {
            _maxGlobalCapacity = activeLevel.maxGlobalCapacityRows;
            _totalActiveRows = 0;

            int conveyorCount = ConveyorList.Count;

            for (int i = 0; i < conveyorCount; i++)
            {
                int nextConveyorIndex = (i + 1) % conveyorCount;
                ConveyorList[i].NextConveyor = ConveyorList[nextConveyorIndex];
                ConveyorList[i].Init(minionPrefab, palette, this);
            }

            GoalFactory.AssignEvent(GetConveyor);
            GoalFactory.BuildGoalLines(palette, activeLevel.goalLines);

            UpdateGlobalCapacityUI();
        }

        public void RegisterRowStateChanged(int delta)
        {
            _totalActiveRows += delta;
            UpdateGlobalCapacityUI();

            if (_totalActiveRows > _maxGlobalCapacity)
            {
                EventBus.RaiseConveyorOverflow();
            }
        }

        private void UpdateGlobalCapacityUI()
        {
            EventBus.RaiseConveyorCapacityChanged(_totalActiveRows, _maxGlobalCapacity);
        }

        public Conveyor GetConveyor(int index)
        {
            if (index >= 0 && index < ConveyorList.Count) return ConveyorList[index];
            return ConveyorList.Count > 0 ? ConveyorList[0] : null;
        }

        public void EndLevel()
        {
            foreach (var conveyor in ConveyorList)
            {
                conveyor.Clear();
            }
        }
    }

}

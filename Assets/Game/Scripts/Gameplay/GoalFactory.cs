using System;
using System.Collections.Generic;
using UnityEngine;
using PeopleFlow.Data;

namespace PeopleFlow.Gameplay
{
    /// <summary>
    /// Factory for managing GoalLines
    /// </summary>
    public class GoalFactory : MonoBehaviour
    {
        private Func<int,Conveyor> _onGetTargetConveyor;
        [SerializeField] private List<GoalLine> _goalLines;
        private List<Conveyor> _conveyors = new List<Conveyor>();
        
        public void BuildGoalLines(ColorPalette palette,List<GoalLineData> goalLineDatas, List<Conveyor> conveyors)
        {
            _conveyors=conveyors;
            // check if data count matches line count, if not create default data
            if (_goalLines.Count > goalLineDatas.Count)
            {
                var cacheDefaultGoalLineDatas = LevelConfigExtensions.CreateDefaultGoalLineData();
                for (int i = 0; i < _goalLines.Count-goalLineDatas.Count; i++)
                {
                    goalLineDatas.Add(cacheDefaultGoalLineDatas);

                }
            }
            for (int i=0; i<_goalLines.Count; i++)
            {
                var line = _goalLines[i];
                // var targetConveyor = _onGetTargetConveyor(goalLineDatas[i].conveyorIndex);

                // Automatic Fallback: Find nearest conveyor if none found by index
                // if (targetConveyor == null)
                // {
                    var targetConveyor = FindNearestConveyor(line.transform.position);
                // }

                line.Build(goalLineDatas[i].gates, targetConveyor, palette, false);
            }
        }

        private Conveyor FindNearestConveyor(Vector3 position)
        {
            Conveyor nearest = null;
            float minDist = float.MaxValue;
            foreach (var conv in _conveyors)
            {
                // Compare distance on XZ plane
                float dist = Vector3.Distance(new Vector3(position.x, 0, position.z), 
                                             new Vector3(conv.transform.position.x, 0, conv.transform.position.z));
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = conv;
                }
            }
            return nearest;
        }

        public void AssignEvent(Func<int,Conveyor> onGetTargetConveyor)
        {
            _onGetTargetConveyor=onGetTargetConveyor;
        }
    }
}

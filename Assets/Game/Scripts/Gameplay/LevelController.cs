using System;
using System.Collections.Generic;
using PeopleFlow.Data;
using UnityEngine;

namespace PeopleFlow.Gameplay
{

    public class LevelController : MonoBehaviour
    {
        public List<Conveyor> ConveyorList;
        public GoalFactory GoalFactory;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public void Init(GameObject minionPrefab, ColorPalette palette, LevelConfig activeLevel)
        {
            int conveyorCount = ConveyorList.Count;

            for (int i = 0; i < conveyorCount; i++)
            {
                int nextConveyorIndex = (i + 1) % conveyorCount;
                ConveyorList[i].NextConveyor = ConveyorList[nextConveyorIndex];
                ConveyorList[i].Init(minionPrefab, palette);
            }
            GoalFactory.AssignEvent(GetConveyor);
            GoalFactory.BuildGoalLines(palette,activeLevel.goalLines);
        }


        public Conveyor GetConveyor(int index)
        {
            if (index >= 0 && index < ConveyorList.Count) return ConveyorList[index];
            return ConveyorList.Count > 0 ? ConveyorList[0] : null;
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
    }
    
}

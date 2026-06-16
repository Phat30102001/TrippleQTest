using System;
using System.Collections.Generic;
using PeopleFlow.Gameplay;
using UnityEngine;

namespace PeopleFlow.Data
{
[CreateAssetMenu(fileName = "LevelListSO", menuName = "PeopleFlow/LevelListSO")]
    public class LevelListSO : ScriptableObject
    {
        public List<LevelSO> LevelSO;
        
        public LevelSO GetLevel(int index) => LevelSO[index];
    }

    [Serializable]
    public class LevelSO
    {
        public LevelConfig LevelConfig;
        public LevelController LevelController;
    }
}
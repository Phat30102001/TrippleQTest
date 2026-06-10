using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using PeopleFlow.Data;

namespace PeopleFlow.EditorTools
{
    /// <summary>
    /// Editor window for rapidly authoring new levels (scales to 100+ levels):
    /// creates a LevelConfig asset pre-filled with a closed conveyor loop and sensible defaults.
    /// </summary>
    public class LevelCreatorWindow : EditorWindow
    {
        private string _levelName = "Level_002";
        private string _folder = "Assets/Game/Data/Levels";
        private float _timeLimit = 40f;
        [Min(1)] public int capacityRows = 8;
        [Min(0.1f)] public float minionSpeed = 3f;
        [Min(0.1f)] public float rowSpacing = 1.5f;
        [Min(1)] public int minionsPerRow = 3;
        private float _rx = 7f, _rz = 5f;

        [MenuItem("PeopleFlow/Level Creator")]
        public static void Open()
        {
            var w = GetWindow<LevelCreatorWindow>("Level Creator");
            w.minSize = new Vector2(340, 360);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("New Level", EditorStyles.boldLabel);
            _levelName = EditorGUILayout.TextField("Name", _levelName);
            _folder = EditorGUILayout.TextField("Folder", _folder);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Conveyor", EditorStyles.boldLabel);
            _timeLimit = EditorGUILayout.FloatField("Time Limit (s)", _timeLimit);
            capacityRows = EditorGUILayout.IntField("Capacity Rows", capacityRows);
            minionSpeed = EditorGUILayout.FloatField("Minion Speed", minionSpeed);
            rowSpacing = EditorGUILayout.FloatField("Row Spacing", rowSpacing);
            minionsPerRow = EditorGUILayout.IntField("Minions / Row",minionsPerRow);
            _rx = EditorGUILayout.FloatField("Loop Radius X", _rx);
            _rz = EditorGUILayout.FloatField("Loop Radius Z", _rz);

            EditorGUILayout.Space();
            if (GUILayout.Button("Create Level Asset", GUILayout.Height(32)))
                CreateLevel();

            EditorGUILayout.HelpBox(
                "Creates a LevelConfig with a closed conveyor loop. Then select the asset and " +
                "use its inspector to add minion queues, goal lines, and locked goal lines.",
                MessageType.Info);
        }

        private void CreateLevel()
        {
            if (!AssetDatabase.IsValidFolder(_folder))
            {
                Debug.LogError("Folder does not exist: " + _folder);
                return;
            }

            var level = ScriptableObject.CreateInstance<LevelConfig>();
            level.timeLimit = _timeLimit;



            // var spline = new Spline();
            // int n = 16;
            // for (int i = 0; i < n; i++)
            // {
            //     float a = (i / (float)n) * Mathf.PI * 2f;
            //     spline.Add(new BezierKnot(new float3(Mathf.Cos(a) * _rx, 0.5f, Mathf.Sin(a) * _rz)), TangentMode.AutoSmooth);
            // }
            // spline.Closed = true;
            // conveyor.path = spline;
            
            // level.conveyors.Add(conveyor);

            string path = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(_folder, _levelName + ".asset").Replace('\\', '/'));
            AssetDatabase.CreateAsset(level, path);
            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(level);
            Selection.activeObject = level;
            // Debug.Log("Created level: " + path);
        }
    }
}

using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using PeopleFlow.Data;

namespace PeopleFlow.EditorTools
{
    /// <summary>
    /// Custom inspector for <see cref="LevelConfig"/> adding convenience tools:
    /// summary stats, a one-click closed-ellipse conveyor generator, and validation.
    /// </summary>
    [CustomEditor(typeof(LevelConfig))]
    public class LevelConfigEditor : Editor
    {
        private float _rx = 7f, _rz = 5f, _ry = 0.5f;
        private int _points = 16;

        public override void OnInspectorGUI()
        {
            var level = (LevelConfig)target;

            EditorGUILayout.LabelField("Level Summary", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.LabelField("Time Limit", level.timeLimit + "s");
                EditorGUILayout.LabelField("Start Columns", level.startColumns.Count.ToString());
                EditorGUILayout.LabelField("Goal Lines", level.goalLines.Count.ToString());
                EditorGUILayout.LabelField("Locked Goal Lines", level.lockedGoalLines.Count.ToString());
                EditorGUILayout.LabelField("Total Goals", level.GetTotalGoalCount().ToString());
                EditorGUILayout.LabelField("Conveyor Count", level.conveyors.Count.ToString());
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Conveyor Path Utils", EditorStyles.boldLabel);
            if (GUILayout.Button("Add Conveyor from Selection"))
            {
                AddConveyorFromSelection(level);
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Conveyor Path Generator (Targets Index 0)", EditorStyles.boldLabel);
            _rx = EditorGUILayout.FloatField("Radius X", _rx);
            _rz = EditorGUILayout.FloatField("Radius Z", _rz);
            _ry = EditorGUILayout.FloatField("Height Y", _ry);
            _points = EditorGUILayout.IntSlider("Points", _points, 6, 32);
            if (GUILayout.Button("Generate Closed Ellipse Loop"))
            {
                Undo.RecordObject(level, "Generate Conveyor Path");
                GenerateEllipse(level);
                EditorUtility.SetDirty(level);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);
            DrawValidation(level);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Raw Data", EditorStyles.boldLabel);
            DrawDefaultInspector();
        }

        private void AddConveyorFromSelection(LevelConfig level)
        {
            var go = Selection.activeGameObject;
            if (go == null)
            {
                Debug.LogWarning("Please select a GameObject with a SplineContainer first.");
                return;
            }

            var container = go.GetComponent<SplineContainer>();
            if (container == null)
            {
                Debug.LogWarning("Selected GameObject does not have a SplineContainer.");
                return;
            }

            Undo.RecordObject(level, "Add Conveyor From Selection");
            
            // var data = new ConveyorData();
            // data.path = new Spline(container.Spline);
            // data.isClosed = container.Spline.Closed;
            //
            // level.conveyors.Add(data);
            // EditorUtility.SetDirty(level);
            // Debug.Log($"Added conveyor with {data.path.Count} knots from {go.name}.");
        }

        private void GenerateEllipse(LevelConfig level)
        {
            if (level.conveyors.Count == 0) level.conveyors.Add(new ConveyorData());
            
            var spline = new Spline();
            for (int i = 0; i < _points; i++)
            {
                float a = (i / (float)_points) * Mathf.PI * 2f;
                float3 p = new float3(Mathf.Cos(a) * _rx, _ry, Mathf.Sin(a) * _rz);
                spline.Add(new BezierKnot(p), TangentMode.AutoSmooth);
            }
            spline.Closed = true;
            level.conveyors[0].path = spline;
        }

        private void DrawValidation(LevelConfig level)
        {
            bool any = false;
            if (level.conveyors.Count == 0)
            {
                EditorGUILayout.HelpBox("No conveyors defined.", MessageType.Error);
                any = true;
            }
            else
            {
                foreach (var conv in level.conveyors)
                {
                    if (conv.path == null || conv.path.Count < 2)
                    {
                        EditorGUILayout.HelpBox("A conveyor path needs at least 2 knots.", MessageType.Error);
                        any = true;
                    }
                }
            }

            if (level.startColumns.Count == 0)
            { EditorGUILayout.HelpBox("No start columns defined.", MessageType.Error); any = true; }

            if (level.GetTotalGoalCount() == 0)
            { EditorGUILayout.HelpBox("No goal gates defined — level cannot be won.", MessageType.Error); any = true; }

            // Check that every minion color has at least one matching gate
            var produced = new System.Collections.Generic.HashSet<MinionColor>();
            foreach (var c in level.startColumns) 
            {
                foreach (var r in c.rows) produced.Add(r.color);
            }
            
            var consumed = new System.Collections.Generic.HashSet<MinionColor>();
            foreach (var l in level.goalLines) foreach (var g in l.gates) consumed.Add(g.color);
            foreach (var l in level.lockedGoalLines) foreach (var g in l.gates) consumed.Add(g.color);
            
            foreach (var col in produced)
                if (!consumed.Contains(col))
                { EditorGUILayout.HelpBox("Color " + col + " is produced but has no matching goal (will clog the belt).", MessageType.Warning); any = true; }

            if (!any) EditorGUILayout.HelpBox("Level looks valid.", MessageType.Info);
        }
    }
}

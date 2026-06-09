using UnityEditor;
using UnityEngine;
using PeopleFlow.Data;

namespace PeopleFlow.EditorTools
{
    [CustomPropertyDrawer(typeof(MinionQueueData))]
    public class MinionQueueDataDrawer : PropertyDrawer
    {
        private static MinionColor _bulkColor = MinionColor.Blue;
        private static int _bulkCount = 10;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label);
            
            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                float y = position.y + EditorGUIUtility.singleLineHeight + 2;

                SerializedProperty conveyorIndexProp = property.FindPropertyRelative("conveyorIndex");
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), conveyorIndexProp);
                y += EditorGUIUtility.singleLineHeight + 2;

                EditorGUI.LabelField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), "Bulk Add Rows", EditorStyles.miniBoldLabel);
                y += EditorGUIUtility.singleLineHeight;

                float controlWidth = position.width;
                float colorWidth = controlWidth * 0.4f;
                float countWidth = controlWidth * 0.3f;
                float buttonWidth = controlWidth * 0.2f;
                
                _bulkColor = (MinionColor)EditorGUI.EnumPopup(new Rect(position.x, y, colorWidth, EditorGUIUtility.singleLineHeight), _bulkColor);
                _bulkCount = EditorGUI.IntField(new Rect(position.x + colorWidth + 5, y, countWidth, EditorGUIUtility.singleLineHeight), _bulkCount);
                
                if (GUI.Button(new Rect(position.x + colorWidth + countWidth + 10, y, buttonWidth, EditorGUIUtility.singleLineHeight), "Add"))
                {
                    SerializedProperty rowsProp = property.FindPropertyRelative("rows");
                    int newIndex = rowsProp.arraySize;
                    rowsProp.InsertArrayElementAtIndex(newIndex);
                    
                    SerializedProperty newElement = rowsProp.GetArrayElementAtIndex(newIndex);
                    newElement.FindPropertyRelative("color").enumValueIndex = (int)_bulkColor;
                    newElement.FindPropertyRelative("count").intValue = _bulkCount;
                    
                    property.serializedObject.ApplyModifiedProperties();
                }
                
                y += EditorGUIUtility.singleLineHeight + 5;

                SerializedProperty rowsPropField = property.FindPropertyRelative("rows");
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUI.GetPropertyHeight(rowsPropField)), rowsPropField, true);

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded) return EditorGUIUtility.singleLineHeight;

            float height = EditorGUIUtility.singleLineHeight + 2; // Foldout
            height += EditorGUIUtility.singleLineHeight + 2; // Conveyor Index
            height += EditorGUIUtility.singleLineHeight; // Label
            height += EditorGUIUtility.singleLineHeight + 5; // Controls
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("rows"), true);
            
            return height;
        }
    }
}
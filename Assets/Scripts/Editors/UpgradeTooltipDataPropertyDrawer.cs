#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(UpgradeTooltipData))]
public class UpgradeTooltipDataPropertyDrawer : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);
        
        var typeProperty = property.FindPropertyRelative("_type");
        var typeRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(typeRect, typeProperty, label);
        var type = (UpgradeTooltipType)typeProperty.enumValueIndex;

        switch (type){
            case UpgradeTooltipType.Text:
                var headerRect = EditorUtils.GetNextLine(position);
                var descriptionRect = EditorUtils.GetNextLine(headerRect);
                
                EditorGUI.PropertyField(headerRect, property.FindPropertyRelative("_header"));
                EditorGUI.PropertyField(descriptionRect, property.FindPropertyRelative("_description"));
                break;
                
            case UpgradeTooltipType.ProgramCard:
                var programCardRect = EditorUtils.GetNextLine(position);
                EditorGUI.PropertyField(programCardRect, property.FindPropertyRelative("_programCard"));
                break;
            
            case UpgradeTooltipType.UpgradeCard:
                var upgradeCardRect = EditorUtils.GetNextLine(position);
                EditorGUI.PropertyField(upgradeCardRect, property.FindPropertyRelative("_upgradeCard"));
                break;
            
            case UpgradeTooltipType.Mechanic:
                var mechanicRect = EditorUtils.GetNextLine(position);
                EditorGUI.PropertyField(mechanicRect, property.FindPropertyRelative("_mechanic"));
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        var type = (UpgradeTooltipType)property.FindPropertyRelative("_type").enumValueIndex;

        switch (type){
            case UpgradeTooltipType.Text:
                return EditorGUIUtility.singleLineHeight * 3;
                                
            case UpgradeTooltipType.ProgramCard:
            case UpgradeTooltipType.UpgradeCard:
            case UpgradeTooltipType.Mechanic:
                return EditorGUIUtility.singleLineHeight * 2;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
#endif
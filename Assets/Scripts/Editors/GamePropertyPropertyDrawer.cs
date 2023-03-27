#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(GameProperty))]
public class GamePropertyPropertyDrawer : PropertyDrawer {
    SerializedProperty _valueProperty;
    SerializedProperty _enabledProperty;
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);

        var gameProperty = property.GetValue<GameProperty>();
        var enabledProperty = property.FindPropertyRelative("_enabled");

        if (gameProperty.HasValue) {
            var valueProperty = property.FindPropertyRelative("_value");
            if (gameProperty.CanToggle) {
                position.width -= 24;   
            }
            EditorGUI.BeginDisabledGroup(!enabledProperty.boolValue);
            EditorGUI.PropertyField(position, valueProperty, label, true);
            EditorGUI.EndDisabledGroup();

            if (gameProperty.CanToggle) {
                position.x += position.width + 8;
                position.width = position.height = EditorGUI.GetPropertyHeight(enabledProperty);
                position.x -= position.width;
                EditorGUI.PropertyField(position, enabledProperty, GUIContent.none);      
            }
        } else {
            EditorGUI.PropertyField(position, enabledProperty, label, true);
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("_value"));
    }
}
#endif
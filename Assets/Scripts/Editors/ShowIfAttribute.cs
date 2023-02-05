#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public enum ShowIfComparer {
    False,
    True,
    Equals,
    NotEquals,
    Greater,
    Less
}

public class ShowIfAttribute : PropertyAttribute {
    public readonly string ConditionalSourceField;
    public readonly ShowIfComparer Comparer;
    public readonly int CompareValue;
    
    public ShowIfAttribute(
        string conditionalSourceField, 
        ShowIfComparer comparer = ShowIfComparer.True,
        int value = 0
        ) {
        ConditionalSourceField = conditionalSourceField;
        Comparer = comparer;
        CompareValue = value;
    }
}

[CustomPropertyDrawer(typeof(ShowIfAttribute))]
public class ShowIfDrawer : PropertyDrawer {
    bool GetEnabled(SerializedObject obj) {
        var showIf = (ShowIfAttribute)attribute;
        var sourceProperty = obj.FindProperty(showIf.ConditionalSourceField);
        
        if (sourceProperty == null) {
            return true;
        }

        return showIf.Comparer switch {
            ShowIfComparer.True => sourceProperty.boolValue,
            ShowIfComparer.False => !sourceProperty.boolValue,
            ShowIfComparer.Equals => sourceProperty.intValue == showIf.CompareValue,
            ShowIfComparer.NotEquals => sourceProperty.intValue != showIf.CompareValue,
            ShowIfComparer.Greater => sourceProperty.intValue > showIf.CompareValue,
            ShowIfComparer.Less => sourceProperty.intValue < showIf.CompareValue,
            _ => true
        };
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return GetEnabled(property.serializedObject) ? EditorGUI.GetPropertyHeight(property, label) : 0;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        if (!GetEnabled(property.serializedObject)) return;
        EditorGUI.PropertyField(position, property, label, true);
    }
}
#endif
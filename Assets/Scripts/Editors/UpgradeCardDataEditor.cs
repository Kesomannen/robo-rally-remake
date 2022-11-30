#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(UpgradeCardData))]
public class UpgradeCardDataEditor : Editor {
    SerializedProperty _nameProperty;
    SerializedProperty _descriptionProperty; 
    SerializedProperty _iconProperty;
    SerializedProperty _costProperty;
    
    SerializedProperty _isActionProperty;
    SerializedProperty _temporaryAffectorsProperty;
    SerializedProperty _permanentAffectorsProperty;
    
    void OnEnable() {
        _nameProperty = GetProperty("_name");
        _descriptionProperty = GetProperty("_description");
        _iconProperty = GetProperty("_icon");
        _costProperty = GetProperty("_cost");

        _isActionProperty = GetProperty("_isAction");
        _temporaryAffectorsProperty = GetProperty("_temporaryAffectors");
        _permanentAffectorsProperty = GetProperty("_permanentAffectors");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();
        
        EditorGUILayout.PropertyField(_nameProperty);
        EditorGUILayout.PropertyField(_descriptionProperty);
        
        EditorGUILayout.Space();
        
        EditorGUILayout.PropertyField(_iconProperty);
        EditorGUILayout.PropertyField(_costProperty);
        
        EditorGUILayout.Space();
        
        EditorGUILayout.PropertyField(_isActionProperty);
        EditorGUILayout.PropertyField(_isActionProperty.boolValue ? _permanentAffectorsProperty : _temporaryAffectorsProperty);

        serializedObject.ApplyModifiedProperties();
    }
    
    SerializedProperty GetProperty(string propertyName) {
        return serializedObject.FindProperty(propertyName);
    }
}
#endif
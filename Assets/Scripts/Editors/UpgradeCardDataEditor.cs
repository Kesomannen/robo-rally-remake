#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(UpgradeCardData))]
public class UpgradeCardDataEditor : Editor {
    SerializedProperty _nameProperty;
    SerializedProperty _descriptionProperty; 
    SerializedProperty _iconProperty;
    SerializedProperty _costProperty;
    SerializedProperty _useCostProperty;
    SerializedProperty _useContextProperty;
    
    SerializedProperty _typeProperty;
    SerializedProperty _temporaryAffectorsProperty;
    SerializedProperty _permanentAffectorsProperty;
    
    SerializedProperty _tooltipsProperty;
    
    void OnEnable() {
        _nameProperty = GetProperty("_name");
        _descriptionProperty = GetProperty("_description");
        _iconProperty = GetProperty("_icon");
        
        _costProperty = GetProperty("_cost");
        _useCostProperty = GetProperty("_useCost");
        _useContextProperty = GetProperty("_useContext");

        _typeProperty = GetProperty("_type");
        _temporaryAffectorsProperty = GetProperty("_temporaryAffectors");
        _permanentAffectorsProperty = GetProperty("_permanentAffectors");
        
        _tooltipsProperty = GetProperty("_tooltips");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();
        var type = (UpgradeType) _typeProperty.enumValueIndex;
        
        EditorGUILayout.PropertyField(_nameProperty);
        EditorGUILayout.PropertyField(_descriptionProperty);
        
        EditorGUILayout.Space();
        
        EditorGUILayout.PropertyField(_iconProperty);
        EditorGUILayout.PropertyField(_costProperty);
        if (type == UpgradeType.Action) EditorGUILayout.PropertyField(_useCostProperty);
        if (type != UpgradeType.Permanent)EditorGUILayout.PropertyField(_useContextProperty);
        
        EditorGUILayout.Space();
        
        EditorGUILayout.PropertyField(_typeProperty);
        EditorGUILayout.PropertyField(
            type == UpgradeType.Permanent
            ? _temporaryAffectorsProperty 
            : _permanentAffectorsProperty, true);

        EditorGUILayout.Space();
        
        EditorGUILayout.PropertyField(_tooltipsProperty);
        
        serializedObject.ApplyModifiedProperties();
    }
    
    SerializedProperty GetProperty(string propertyName) {
        return serializedObject.FindProperty(propertyName);
    }
}
#endif
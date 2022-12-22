# if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class EditorUtils {
    const float DefaultLineSpacing = 1f;
    
    public static Rect GetNextLine(Rect rect, float spacing = DefaultLineSpacing) {
        return new Rect(rect.x, rect.y + spacing + EditorGUIUtility.singleLineHeight, rect.width, EditorGUIUtility.singleLineHeight);
    }
}
# endif
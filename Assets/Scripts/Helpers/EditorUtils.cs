# if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class EditorUtils {
    public static Rect GetNextLine(Rect rect) {
        return new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight, rect.width, EditorGUIUtility.singleLineHeight);
    }
}
# endif
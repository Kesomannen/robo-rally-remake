using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapData))]
public class MapDataEditor : Editor {
    MapData data;

    void OnEnable() {
        data = (MapData) target;
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        if (data.Prefab != null) {
            var texture = AssetPreview.GetAssetPreview(data.Prefab);
            GUILayout.Label("", GUILayout.Height(80), GUILayout.Width(80));
            GUI.DrawTexture(GUILayoutUtility.GetLastRect(), texture);
        }
    }
}
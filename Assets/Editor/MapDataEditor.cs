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
        var texture = AssetPreview.GetAssetPreview(data.Prefab.gameObject);
        if (texture != null) {
            GUILayout.Label("", GUILayout.Height(100), GUILayout.Width(100));
            GUI.DrawTexture(GUILayoutUtility.GetLastRect(), texture);
        }
    }
}
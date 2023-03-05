#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using static System.IO.Directory;
using static System.IO.File;

[CustomEditor(typeof(MapData))]
public class MapDataEditor : Editor {
    static readonly Vector2Int _size = new(960, 720);
    const int Depth = 24;
    const float Margin = 0f;
    const string AssetPath = "Textures/MapThumbnails";

    public override void OnInspectorGUI() { 
        base.OnInspectorGUI();

        if (GUILayout.Button("Generate Thumbnail")) {
            var mapData = (MapData)target;
            mapData.Thumbnail = GenerateThumbnail(mapData);
        } else if (GUILayout.Button("Generate All Thumbnails")) {
            foreach (var mapData in MapData.GetAll()) {
                mapData.Thumbnail = GenerateThumbnail(mapData);
            }
        }
    }
    
    static Sprite GenerateThumbnail(MapData map) {
        var mapObject = Instantiate(map.Prefab, Vector3.zero, Quaternion.identity);
        foreach (var component in mapObject.GetComponentsInChildren<ITriggerAwake>()) {
            component.TriggerAwake();
        }
        var tilemaps = mapObject.GetComponentsInChildren<Tilemap>();
        
        var aspect = _size.x / (float) _size.y;
        var (camSize, center) = MapSystem.GetCameraPosition(tilemaps, aspect, Margin);

        var cameraObject = new GameObject("Camera") {
            transform = {
                position = new Vector3(center.x, center.y, -10)
            }
        };
        var cam = cameraObject.AddComponent<Camera>();

        cam.aspect = aspect;
        cam.orthographic = true;
        cam.orthographicSize = camSize;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.clear;

        var renderTexture = new RenderTexture(_size.x, _size.y, Depth);
        var rect = new Rect(0,0,_size.x,_size.y);
        var texture = new Texture2D(_size.x, _size.y, TextureFormat.RGBA32, false);

        cam.targetTexture = renderTexture;
        cam.Render();

        var currentRenderTexture = RenderTexture.active;
        RenderTexture.active = renderTexture;
        texture.ReadPixels(rect, 0, 0);
        texture.Apply();

        cam.targetTexture = null;
        RenderTexture.active = currentRenderTexture;
        
        DestroyImmediate(renderTexture);
        DestroyImmediate(cameraObject);
        DestroyImmediate(mapObject);

        var projectPath = Path.Combine(AssetPath, $"{map.name}.png");
        var absolutePath = Path.Combine(Application.dataPath, projectPath);
        projectPath = Path.Combine("Assets", projectPath);

        CreateDirectory(Path.GetDirectoryName(absolutePath)!);
        WriteAllBytes(absolutePath, texture.EncodeToPNG());

        AssetDatabase.Refresh();
        var textureImporter = (TextureImporter) AssetImporter.GetAtPath(projectPath);
        textureImporter.textureType = TextureImporterType.Sprite;
        textureImporter.spriteImportMode = SpriteImportMode.Single;
        
        EditorUtility.SetDirty(textureImporter);
        textureImporter.SaveAndReimport();
        
        return AssetDatabase.LoadAssetAtPath<Sprite>(projectPath);
    }
}
#endif

public interface ITriggerAwake {
    void TriggerAwake();
}
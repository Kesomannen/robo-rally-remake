using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapSystem : Singleton<MapSystem> {
    [SerializeField] Grid _grid;
    [SerializeField] Camera _mapCamera;
    [SerializeField] float _cameraMargin = 1f;
    
    static Dictionary<Vector2Int, List<MapObject>> _tiles;
    static Dictionary<Tilemap, IBoard> _boards;
    
    GameObject _currentMap;

    public static Action MapLoaded;

    public void LoadMap(MapData mapData) {
        if (_currentMap != null) {
            Destroy(_currentMap);
        }
        
        _currentMap = Instantiate(mapData.Prefab, _grid.transform);
        
        var tilemaps = _currentMap.GetComponentsInChildren<Tilemap>();
        _boards = tilemaps.ToDictionary(x => x, x => x.GetComponentInParent<IBoard>());

        // Register MapObjects
        _tiles = new Dictionary<Vector2Int, List<MapObject>>();
        var onEnterHandlers = new List<IOnEnterHandler>();
        foreach (var obj in _grid.GetComponentsInChildren<MapObject>()) {
            RegisterMapObject(obj);
            if (obj is IOnEnterHandler handler) onEnterHandlers.Add(handler);
        }

        // Call OnEnter handlers for objects on the map
        foreach (var handler in onEnterHandlers) {
            foreach (var obj in _tiles[handler.Object.GridPos].Drop(handler.Object)) {
                handler.OnEnter(obj);
            }
        }

        PositionCamera();
        
        MapLoaded?.Invoke();
        Debug.Log($"Map {mapData} loaded.");
    }
    
    public static (float Size, Vector2 Pos) GetCameraPosition(IEnumerable<Tilemap> tilemaps, float aspectRatio, float margin) {
        var mapMax = Vector2.negativeInfinity;
        var mapMin = Vector2.positiveInfinity;

        foreach (var tilemap in tilemaps) {
            tilemap.CompressBounds();
            var bounds = tilemap.cellBounds;
            var boundsMax = (Vector2)tilemap.CellToWorld(bounds.max);
            var boundsMin = (Vector2)tilemap.CellToWorld(bounds.min);
            
            // Correcting bounds for tilemap
            var max = Vector2.Max(boundsMax, boundsMin);
            var min = Vector2.Min(boundsMax, boundsMin);
            
            mapMax = Vector2.Max(mapMax, max);
            mapMin = Vector2.Min(mapMin, min);
        }

        var verticalMin = (mapMax.y - mapMin.y) / 2;
        var horizontalMin = (mapMax.x - mapMin.x) / 2;
        
        float size;
        if (verticalMin * aspectRatio < horizontalMin) {
            size = horizontalMin / aspectRatio + margin;
        } else {
            size = verticalMin + margin;
        }

        var center = (mapMax + mapMin) / 2;
        return (size, center);
    }
    
    void PositionCamera() {
        var (size, pos) = GetCameraPosition(_boards.Keys, _mapCamera.aspect, _cameraMargin);
        
        var t = _mapCamera.transform;
        t.position = new Vector3(pos.x, pos.y, t.position.z);
        _mapCamera.orthographicSize = size;
    }

    void RegisterMapObject(MapObject mapObject) {
        mapObject.GridPos = WorldToGrid(mapObject.transform.position);
        _tiles.EnforceKey(mapObject.GridPos, () => new List<MapObject>()).Add(mapObject);
    }

    void AddObject(MapObject mapObject, Vector2Int gridPos, bool callOnEnter = true) {
        mapObject.GridPos = gridPos;
        if (TryGetBoard(gridPos, out var board)) {
            board.Parent(mapObject.transform);
        } else {
            mapObject.Fall(GetParentBoard(mapObject));
        }

        var tile = _tiles.EnforceKey(gridPos, () => new List<MapObject>());
        tile.Add(mapObject);

        if (callOnEnter){
            CallHandlers<IOnEnterHandler>(tile, obj => obj.OnEnter(mapObject), mapObject);
        }
    }

    static void RemoveObject(MapObject mapObject, bool callOnExit = true) {
        if (!_tiles.ContainsKey(mapObject.GridPos)) return;
        
        var tile = _tiles[mapObject.GridPos];
        tile.Remove(mapObject);
        
        if (callOnExit){
            CallHandlers<IOnExitHandler>(tile, obj => obj.OnExit(mapObject));
        }
    }

    static void CallHandlers<T>(IEnumerable<MapObject> tile, Action<T> action, MapObject except = null) where T : IMapObject {
        var handlers = except == null ? tile.OfType<T>().ToArray() : tile.Where(x => x != except).OfType<T>().ToArray();
        // ReSharper disable once ForCanBeConvertedToForeach
        // Collection was modified; enumeration may not operate correctly
        for (var i = 0; i < handlers.Length; i++) {
            var handler = handlers[i];
            action(handler);
        }
    }

    public void MoveObjectInstant(MapObject mapObject, Vector2Int gridPos) {
        RelocateObject(mapObject, gridPos);
        mapObject.transform.position = GridToWorld(gridPos);
    }

    public void RelocateObject(MapObject mapObject, Vector2Int gridPos) {
        RemoveObject(mapObject);
        AddObject(mapObject, gridPos);
    }

    public T CreateObject<T>(T prefab, Vector2Int gridPos, Quaternion rotation, bool callOnEnter = true) where T : MapObject {
        var obj = Instantiate(prefab, position: Vector3.zero, rotation: rotation);
        
        var onMap = TryGetBoard(gridPos, out var board);
        if (!onMap) Debug.LogWarning($"Object {obj} was created off the map at {gridPos}.");
        board.Parent(obj.transform);
        
        obj.transform.position = GridToWorld(gridPos);
        AddObject(obj, gridPos, callOnEnter);

        return obj;
    }

    public T CreateObject<T>(T prefab, Vector2Int gridPos, IBoard parent, bool callOnEnter = true) where T : MapObject {
        var obj = Instantiate(prefab);
        parent.Parent(obj.transform);
        
        obj.transform.position = GridToWorld(gridPos);
        AddObject(obj, gridPos, callOnEnter);

        return obj;
    }

    public static void DestroyObject(MapObject obj, bool callOnExit = true) {
        RemoveObject(obj, callOnExit);
        Destroy(obj.gameObject);
    }

    public static bool TryGetTile(Vector2Int gridPos, out IReadOnlyList<MapObject> result){
        var success = _tiles.TryGetValue(gridPos, out var tile);
        result = tile;
        return success;
    }

    public static IReadOnlyList<MapObject> GetTile(Vector2Int gridPos){
        return TryGetTile(gridPos, out var result) ? result : null;
    }

    Vector2Int WorldToGrid(Vector2 worldPos) {
        return _grid.WorldToCell(worldPos).ToVec2Int();
    }
    
    public Vector2 GridToWorld(Vector2Int gridPos) {
        return _grid.CellToWorld(gridPos.ToVec3Int());
    }

    public static IBoard GetParentBoard(MapObject mapObject) {
        return mapObject.GetComponentInParent<IBoard>();
    }

    public bool TryGetBoard(Vector2Int gridPos, out IBoard board) {
        foreach (var (tilemap, b) in _boards) {
            var pos = tilemap.WorldToCell(GridToWorld(gridPos));
            pos.z = 0;
            if (!tilemap.HasTile(pos)) continue;
            
            board = b;
            return true;
        }
        board = null;
        return false;
    }

    public static IEnumerable<T> GetByType<T>() where T : MapObject {
        return _tiles.Values.SelectMany(tile => tile).OfType<T>();
    }
}
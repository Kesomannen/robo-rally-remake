using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

# pragma warning disable 0252

public class MapSystem : Singleton<MapSystem> {
    [SerializeField] Grid _grid;
    [SerializeField] Camera _mapCamera;
    [SerializeField] float _cameraMargin = 1f;
    [SerializeField] [ReadOnly] Vector2 _mapMax;
    [SerializeField] [ReadOnly] Vector2 _mapMin;
    [SerializeField] [ReadOnly] Vector2 _mapCenter;

    static Dictionary<Vector2Int, List<MapObject>> _tiles;
    static Dictionary<Tilemap, IBoard> _boards;
    
    GameObject _currentMap;
    (Vector2 Min, Vector2 Max)[] _tilemapBounds;

    public static Action OnMapLoaded;

    public void LoadMap(MapData mapData) {
        Debug.Log($"Loading map {mapData}...");

        if (_currentMap != null) {
            Destroy(_currentMap);
        }
        
        _currentMap = Instantiate(mapData.Prefab, _grid.transform);

        // Find tilemaps and boards
        var tilemaps = _currentMap.GetComponentsInChildren<Tilemap>();
        _boards = tilemaps.ToDictionary(x => x, x => x.GetComponentInParent<IBoard>());
        Debug.Log($"Registered {_boards.Count} boards.");

        // Register MapObjects
        _tiles = new Dictionary<Vector2Int, List<MapObject>>();
        var onEnterHandlers = new List<IOnEnterHandler>();
        foreach (var obj in _grid.GetComponentsInChildren<MapObject>(true)) {
            RegisterMapObject(obj);
            if (obj is IOnEnterHandler handler) onEnterHandlers.Add(handler);
        }
        Debug.Log($"Registered {_tiles.Count} tiles.");

        // Call OnEnter handlers
        foreach (var handler in onEnterHandlers) {
            foreach (var obj in _tiles[handler.Object.GridPos].Drop(handler.Object)) {
                handler.OnEnter(obj);
            }
        }
        Debug.Log($"Called OnEnter for {onEnterHandlers.Count} objects.");
        
        PositionCamera();
        
        OnMapLoaded?.Invoke();
        Debug.Log($"Map {mapData} loaded.");
    }

    void OnDrawGizmos() {
        if (!Application.isPlaying) return;
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(_mapCenter, (_mapMax - _mapMin) + 0.5f * Vector2.one);

        Gizmos.color = Color.green;
        foreach (var (min, max) in _tilemapBounds) {
            Gizmos.DrawWireCube((max + min) / 2, max - min);
        }
    }

    void PositionCamera() {
        var t = _mapCamera.transform;
        _mapMax = Vector2.negativeInfinity;
        _mapMin = Vector2.positiveInfinity;
        
        _tilemapBounds = new (Vector2 Min, Vector2 Max)[_boards.Count];

        var tilemaps = _boards.Keys.ToArray();
        
        for (var i = 0; i < tilemaps.Length; i++) {
            var tilemap = tilemaps[i];
            
            tilemap.CompressBounds();
            var bounds = tilemap.cellBounds;
            var boundsMax = (Vector2)tilemap.CellToWorld(bounds.max);
            var boundsMin = (Vector2)tilemap.CellToWorld(bounds.min);
            
            // Correcting bounds for tilemap
            var max = Vector2.Max(boundsMax, boundsMin);
            var min = Vector2.Min(boundsMax, boundsMin);
            _tilemapBounds[i] = (min, max);

            Debug.Log($"Tilemap {tilemap.name} bounds: {min} - {max}", tilemap);
            _mapMax = Vector2.Max(_mapMax, max);
            _mapMin = Vector2.Min(_mapMin, min);
        }

        var verticalMin = (_mapMax.y - _mapMin.y) / 2;
        var horizontalMin = (_mapMax.x - _mapMin.x) / 2;
        
        if (verticalMin * _mapCamera.aspect < horizontalMin) {
            _mapCamera.orthographicSize = horizontalMin / _mapCamera.aspect + _cameraMargin;
        } else {
            _mapCamera.orthographicSize = verticalMin + _cameraMargin;
        }

        _mapCenter = (_mapMax + _mapMin) / 2;
        t.position = new Vector3(_mapCenter.x, _mapCenter.y, t.position.z);
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
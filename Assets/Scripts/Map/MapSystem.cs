using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

# pragma warning disable 0252

public class MapSystem : Singleton<MapSystem> {
    [SerializeField] Grid _grid;

    static Dictionary<Vector2Int, List<MapObject>> _tiles;
    static Dictionary<Tilemap, IBoard> _boards;
    
    GameObject _currentMap;

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
        foreach (var obj in _grid.GetComponentsInChildren<MapObject>(true)) {
            RegisterMapObject(obj);
        }
        Debug.Log($"Registered {_tiles.Count} tiles.");

        mapData.OnLoad();
        OnMapLoaded?.Invoke();
        Debug.Log($"Map {mapData} loaded.");
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

    void RemoveObject(MapObject mapObject, bool callOnExit = true) {
        if (!_tiles.ContainsKey(mapObject.GridPos)) return;
        
        var tile = _tiles[mapObject.GridPos];
        tile.Remove(mapObject);
        
        if (callOnExit){
            CallHandlers<IOnExitHandler>(tile, obj => obj.OnExit(mapObject));
        }
    }

    static void CallHandlers<T>(IEnumerable<MapObject> tile, Action<T> action, MapObject except = null) where T : IMapObject {
        var handlers = except == null ? tile.OfType<T>() : tile.Where(x => x != except).OfType<T>();
        foreach (var handler in handlers) action(handler);
    }

    public void MoveObjectInstant(MapObject mapObject, Vector2Int gridPos) {
        RelocateObject(mapObject, gridPos);
        mapObject.transform.position = GridToWorld(gridPos);
    }

    public void RelocateObject(MapObject mapObject, Vector2Int gridPos) {
        RemoveObject(mapObject);
        AddObject(mapObject, gridPos);
    }

    public T CreateObject<T>(T prefab, Vector2Int gridPos, bool callOnEnter = true) where T : MapObject {
        var obj = Instantiate(prefab);
        
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

    public void DestroyObject(MapObject obj, bool callOnExit = true) {
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
    
    public Vector2Int WorldToGrid(Vector3 worldPos) {
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
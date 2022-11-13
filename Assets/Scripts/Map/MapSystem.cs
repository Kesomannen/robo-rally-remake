using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

# pragma warning disable 0252

public class MapSystem : Singleton<MapSystem> {
    [SerializeField] Grid _grid;

    const float _mapObjectMoveSpeed = 2f;

    static Dictionary<Vector2Int, List<MapObject>> _tiles;
    static Dictionary<Tilemap, IBoard> _boards;

    MapData _currentMapData;
    GameObject _currentMap;

    public static Action OnMapLoaded;

    public void LoadMap(MapData mapData) {
        Debug.Log($"Loading map {mapData}...");

        if (_currentMap != null) {
            Destroy(_currentMap);
        }

        _currentMapData = mapData;
        _currentMap = Instantiate(mapData.Prefab, _grid.transform);

        // Find tilemaps and boards
        var tilemaps = _currentMap.GetComponentsInChildren<Tilemap>();
        _boards = tilemaps.ToDictionary(x => x, x => x.GetComponentInParent<IBoard>());
        Debug.Log($"Registered {_boards.Count} boards.");

        // Register MapObjects
        _tiles = new();
        foreach (var obj in _grid.GetComponentsInChildren<MapObject>(true)) {
            RegisterMapObject(obj);
        }
        Debug.Log($"Registered {_tiles.Count} map objects.");

        mapData.OnLoad();
        OnMapLoaded?.Invoke();
        Debug.Log($"Map {mapData} loaded.");
    }

    void RegisterMapObject(MapObject mapObject) {
        _tiles.EnforceKey(mapObject.GridPos, () => new()).Add(mapObject);
        mapObject.GridPos = WorldToGrid(mapObject.transform.position);
    }

    void AddObject(MapObject mapObject, Vector2Int gridPos) {
        var tile = _tiles.EnforceKey(gridPos, () => new());
        
        tile.Add(mapObject);
        mapObject.GridPos = gridPos;

        CallHandlers<IOnEnterHandler>(tile, obj => obj.OnEnter(mapObject), mapObject);
    }

    bool RemoveObject(MapObject mapObject) {
        if (!_tiles.ContainsKey(mapObject.GridPos)) return false;
        
        var tile = _tiles[mapObject.GridPos];
        if (tile.Remove(mapObject)) {
            CallHandlers<IOnExitHandler>(tile, obj => obj.OnExit(mapObject));
            return true;
        }
        return false;
    }
    
    void CallHandlers<T>(List<MapObject> tile, Action<T> action, MapObject except = null) where T : IMapObject {
        var handlers = except == null ? tile.OfType<T>() : tile.Where(x => x != except).OfType<T>();
        foreach (var handler in handlers) action(handler);
    }

    public bool MoveObject(MapObject movable, Vector2Int gridPos) {
        var mapObject = movable.Object;
        if (RemoveObject(mapObject)) {
            AddObject(mapObject, gridPos);
            return true;
        } else {
            return false;
        }
    }

    public T CreateObject<T>(T prefab, Vector2Int gridPos, bool callOnEnter = true) where T : MapObject {
        var obj = Instantiate(prefab, _grid.transform);

        if (callOnEnter) {
            AddObject(obj, gridPos);
        } else {
            RegisterMapObject(obj);
        }
        
        return obj;
    }

    public bool DestroyObject(MapObject obj) {
        if (RemoveObject(obj)) {
            Destroy(obj.gameObject);
            return true;
        }
        return false;
    }

    public bool TryGetTile(Vector2Int gridPos, out IReadOnlyList<MapObject> result) {
        if (_tiles.ContainsKey(gridPos)) {
            result = _tiles[gridPos];
            return true;
        } else {
            result = null;
            return false;
        }
    }
    
    public Vector2Int WorldToGrid(Vector3 worldPos) {
        return _grid.WorldToCell(worldPos).ToVec2Int();
    }
    
    public Vector2 GridToWorld(Vector2Int gridPos) {
        return _grid.CellToWorld(gridPos.ToVec3Int());
    }

    public bool TryGetBoard(Vector2Int gridPos, out IBoard board) {
        foreach (var (tilemap, b) in _boards) {
            var pos = tilemap.WorldToCell(GridToWorld(gridPos));
            if (tilemap.HasTile(pos)) {
                board = b;
                return true;
            }
        }
        board = null;
        return false;
    }
}
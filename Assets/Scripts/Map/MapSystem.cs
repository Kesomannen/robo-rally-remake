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
        Debug.Log($"Registered {_tiles.Count} tiles.");

        mapData.OnLoad();
        OnMapLoaded?.Invoke();
        Debug.Log($"Map {mapData} loaded.");
    }

    void RegisterMapObject(MapObject mapObject) {
        mapObject.GridPos = WorldToGrid(mapObject.transform.position);
        _tiles.EnforceKey(mapObject.GridPos, () => new()).Add(mapObject);
    }

    void AddObject(MapObject mapObject, Vector2Int gridPos) {
        mapObject.GridPos = gridPos;
        if (TryGetBoard(gridPos, out var board)) {
            board.Parent(mapObject.transform);
        } else {
            mapObject.Fall(GetParentBoard(mapObject));
        }

        var tile = _tiles.EnforceKey(gridPos, () => new());
        tile.Add(mapObject);

        CallHandlers<IOnEnterHandler>(tile, obj => obj.OnEnter(mapObject), mapObject);
    }

    void RemoveObject(MapObject mapObject) {
        if (!_tiles.ContainsKey(mapObject.GridPos)) return;
        
        var tile = _tiles[mapObject.GridPos];
        tile.Remove(mapObject);
        CallHandlers<IOnExitHandler>(tile, obj => obj.OnExit(mapObject));
    }
    
    void CallHandlers<T>(List<MapObject> tile, Action<T> action, MapObject except = null) where T : IMapObject {
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
        var obj = Instantiate(prefab, _grid.transform);
        obj.transform.position = GridToWorld(gridPos);

        if (callOnEnter) {
            AddObject(obj, gridPos);
        } else {
            RegisterMapObject(obj);
        }
        
        return obj;
    }

    public void DestroyObject(MapObject obj) {
        RemoveObject(obj);
        Destroy(obj.gameObject);
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

    public IReadOnlyList<MapObject> GetTile(Vector2Int gridPos) {
        if (TryGetTile(gridPos, out var result)) return result;
        else return null;
    }
    
    public Vector2Int WorldToGrid(Vector3 worldPos) {
        return _grid.WorldToCell(worldPos).ToVec2Int();
    }
    
    public Vector2 GridToWorld(Vector2Int gridPos) {
        return _grid.CellToWorld(gridPos.ToVec3Int());
    }

    public IBoard GetParentBoard(MapObject mapObject) {
        return mapObject.GetComponentInParent<IBoard>();
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

    public void GetByType<T>(List<T> outputList) where T : MapObject {
        foreach (var tile in _tiles.Values) {
            foreach (var obj in tile) {
                if (obj is T t) outputList.Add(t);
            }
        }
    }
}
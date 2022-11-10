using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapSystem : Singleton<MapSystem> {
    [SerializeField] Grid _grid;

    const float _mapObjectMoveSpeed = 1f;

    static Dictionary<Vector2Int, List<MapObject>> _mapObjects;

    MapData _currentMapData;
    Tilemap _currentMap;

    public static Action OnMapLoaded;

    public void LoadMap(MapData mapData) {
        Debug.Log($"Loading map {mapData}...");

        if (_currentMap != null) {
            Destroy(_currentMap);
        }

        _currentMapData = mapData;
        _currentMap = Instantiate(mapData.Prefab, _grid.transform);

        _mapObjects = new();
        foreach (var obj in _grid.GetComponentsInChildren<MapObject>(true)) {
            _mapObjects.EnforceKey(obj.GridPos, () => new()).Add(obj);
        }

        mapData.OnLoad();
        OnMapLoaded?.Invoke();
    }

    void AddObject(DynamicObject obj, Vector2Int gridPosition) {
        var tile = _mapObjects.EnforceKey(obj.GridPos, () => new());
        tile.ForEach(t => t.OnEnter(obj));
        tile.Add(obj);

        if (!_currentMap.HasTile(gridPosition.ToVec3Int())) {
            Debug.Log($"Object {obj} is outside of map");
            obj.Fall();
        }
    }

    void RemoveObject(DynamicObject obj, Vector2Int gridPosition) {
        if (!_mapObjects.ContainsKey(gridPosition)) return;
        var tile = _mapObjects[gridPosition];
        tile.Remove(obj);
        tile.ForEach(t => t.OnExit(obj));
    }

    void RelocateObject(DynamicObject obj, Vector2Int newPosition) {
        RemoveObject(obj, GetGridPos(obj));
        AddObject(obj, newPosition);
    }

    # region Public Methods
    public T CreateObject<T>(T prefab, Vector2Int gridPosition) where T : DynamicObject {
        var mapObject = Instantiate(prefab, _grid.transform);
        mapObject.transform.position = GetWorldPos(gridPosition);
        AddObject(mapObject, gridPosition);
        return mapObject;
    }

    public T CreateObject<T>(T prefab, Vector3 worldPosition) where T : DynamicObject {
        return CreateObject(prefab, GetGridPos(worldPosition));
    }

    public void DestroyObject(DynamicObject obj, Vector2Int gridPosition) {
        RemoveObject(obj, gridPosition);
        Destroy(obj.gameObject);
    }

    public void MoveObjectInstant(DynamicObject obj, Vector2Int newPosition) {
        RelocateObject(obj, newPosition);
        obj.transform.position = GetWorldPos(newPosition);
    }

    public IEnumerator MoveObjectRoutine(DynamicObject obj, Vector2Int newPosition) {
        RelocateObject(obj, newPosition);
        LeanTween.move(obj.gameObject, GetWorldPos(newPosition), 1 / _mapObjectMoveSpeed);
        yield return Helpers.Wait(1 / _mapObjectMoveSpeed);
    }

    public Vector2 GetWorldPos(Vector2Int gridPosition) {
        return _grid.CellToWorld(gridPosition.ToVec3Int());
    }

    public Vector2Int GetGridPos(MapObject obj) {
        return _grid.WorldToCell(obj.transform.position).ToVec2Int();
    }

    public Vector2Int GetGridPos(Vector2 worldPosition) {
        return _grid.WorldToCell(worldPosition).ToVec2Int();
    }

    public bool TryGetTile(Vector2Int gridPosition, out IReadOnlyList<MapObject> tile) {
        if (_mapObjects.ContainsKey(gridPosition)) {
            tile = _mapObjects[gridPosition];
            return true;
        } else {
            tile = null;
            return false;
        }
    }

    public void GetByType<T>(List<T> outputList) where T : MapObject {
        foreach (var tile in _mapObjects.Values) {
            foreach (var obj in tile) {
                if (obj is T t) {
                    outputList.Add(t);
                }
            }
        }
    }

    public Board GetBoard(StaticObject obj) {
        return obj.GetComponentInParent<Board>();
    }

    public Board GetBoard(DynamicObject obj) {
        throw new NotImplementedException();
    }
    #endregion
}
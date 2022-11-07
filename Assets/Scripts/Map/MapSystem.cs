using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSystem : Singleton<MapSystem> {
    [SerializeField] float _moveSpeed = 1f;

    static Dictionary<Vector2Int, List<MapObject>> _mapObjects;

    MapData _currentMapData;
    Grid _grid;

    public static Action OnMapLoaded;

    public void LoadMap(MapData mapData) {
        Debug.Log($"Loading map {mapData.name}...");

        _currentMapData = mapData;
        _grid = Instantiate(mapData.Prefab, transform);

        _mapObjects = new();
        foreach (var obj in _grid.GetComponentsInChildren<MapObject>(true)) {
            EnforceTile(obj.GridPos).Add(obj);
        };

        mapData.OnLoad();
        OnMapLoaded?.Invoke();
    }

    void AddObject(DynamicObject obj, Vector2Int gridPosition) {
        var tile = EnforceTile(gridPosition);
        tile.ForEach(t => t.OnEnter(obj));
        tile.Add(obj);
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

    List<MapObject> EnforceTile(Vector2Int gridPosition) {
        if (!_mapObjects.ContainsKey(gridPosition)) {
            _mapObjects.Add(gridPosition, new());
        }
        return _mapObjects[gridPosition];
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
        LeanTween.move(obj.gameObject, GetWorldPos(newPosition), 1 / _moveSpeed);
        yield return Helpers.Wait(1 / _moveSpeed);
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
    # endregion
}
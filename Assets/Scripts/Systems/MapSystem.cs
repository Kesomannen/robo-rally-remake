using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSystem : Singleton<MapSystem> {
    [SerializeField] Grid _grid;
    [SerializeField] float _moveSpeed = 1f;

    readonly static Dictionary<Vector2Int, List<MapObject>> _mapObjects = new();

    const int maxX = 5;
    const int maxY = 5;
    const int minX = -5;
    const int minY = -5;

    protected override void Awake() {
        base.Awake();
        LoadTiles();
    }

    void LoadTiles() {
        foreach (var obj in _grid.GetComponentsInChildren<MapObject>(true)) {
            RegisterObject(obj);
        };
    }

    void RegisterObject(MapObject mapObject) {
        EnforceTile(mapObject.GridPos).Add(mapObject);
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

    # region Public API
    public DynamicObject CreateObject(DynamicObject prefab, Vector2Int gridPosition) {
        var mapObject = Instantiate(prefab, _grid.transform);
        mapObject.transform.position = GetWorldPos(gridPosition);
        AddObject(mapObject, gridPosition);
        return mapObject;
    }

    public DynamicObject CreateObject(DynamicObject prefab, Vector3 worldPosition) {
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

    public IEnumerator MoveObject(DynamicObject obj, Vector2Int newPosition) {
        RelocateObject(obj, newPosition);
        LeanTween.move(obj.gameObject, GetWorldPos(newPosition), 1 / _moveSpeed);
        yield return new WaitForSeconds(1 / _moveSpeed);
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

    public bool TryGetTile(Vector2Int gridPosition, out List<MapObject> tile) {
        if (_mapObjects.ContainsKey(gridPosition)) {
            tile = _mapObjects[gridPosition];
            return true;
        } else {
            tile = null;
            return false;
        }
    }

    public Vector2Int GetRandomEmptyGridPos() {
        Vector2Int pos;
        do { pos = new Vector2Int(Random.Range(minX, maxX), Random.Range(minY, maxY)); }
        while (!Evaluate(pos));
        return pos;

        bool Evaluate(Vector2Int pos) {
            if (!TryGetTile(pos, out var tile)) return true;
            return tile.Count == 0;
        }
    }
    # endregion
}
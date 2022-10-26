using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSystem : Singleton<MapSystem> {
    [SerializeField] Grid _grid;
    [SerializeField] float _moveSpeed = 1f;

    Dictionary<Vector2Int, List<MapObject>> _mapObjects;

    const int maxX = 5;
    const int maxY = 5;
    const int minX = -5;
    const int minY = -5;

    protected override void Awake() {
        base.Awake();
        LoadTiles();
    }

    void LoadTiles() {
        _mapObjects = new();
        var objects = _grid.GetComponentsInChildren<MapObject>(true);

        foreach (var obj in objects) {
            _mapObjects.EnforceKey(obj.GetGridPos(), () => new()).Add(obj);
        }
    }

    void AddMapObject(MapObject obj, Vector2Int gridPosition) {
        var tile = _mapObjects.EnforceKey(gridPosition, () => new());
        tile.ForEach(t => t.OnEnter(obj));
        tile.Add(obj);
    }

    void RemoveMapObject(MapObject obj, Vector2Int gridPosition) {
        if (!_mapObjects.ContainsKey(gridPosition)) return;
        var tile = _mapObjects[gridPosition];
        tile.Remove(obj);
        tile.ForEach(t => t.OnExit(obj));
    }

    void RelocateTile(MapObject obj, Vector2Int newPosition) {
        RemoveMapObject(obj, GetGridPos(obj));
        AddMapObject(obj, newPosition);
    }

    public MapObject CreateMapObject(MapObject prefab, Vector2Int gridPosition) {
        var mapObject = Instantiate(prefab, _grid.transform);
        mapObject.transform.position = GetWorldPos(gridPosition);
        AddMapObject(mapObject, gridPosition);
        return mapObject;
    }

    public void DestoryMapObject(MapObject obj, Vector2Int gridPosition) {
        RemoveMapObject(obj, gridPosition);
        Destroy(obj.gameObject);
    }

    public void MoveMapObjectInstant(MapObject obj, Vector2Int newPosition) {
        RelocateTile(obj, newPosition);
        obj.transform.position = GetWorldPos(newPosition);
    }

    public IEnumerator MoveMapObject(MapObject obj, Vector2Int newPosition) {
        RelocateTile(obj, newPosition);
        Debug.Log($"Moving {obj.name} to {newPosition}");
        LeanTween.move(obj.gameObject, GetWorldPos(newPosition), 1 / _moveSpeed);
        yield return new WaitForSeconds(1 / _moveSpeed);
    }

    public void MoveTileInstant(List<MapObject> tile, Vector2Int newPosition) {
        tile.ForEach(mapObj => MoveMapObject(mapObj, newPosition));
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
}
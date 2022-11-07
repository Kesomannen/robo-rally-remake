using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Interaction {
    public static bool CanEnter(IReadOnlyList<MapObject> tile, Vector2Int dir) {
        foreach (var obj in tile) {
            if (!obj.CanEnter(dir)) return false;
        }
        return true;
    }

    public static bool CanEnter(Vector2Int gridPos, Vector2Int dir) {
        return !MapSystem.Instance.TryGetTile(gridPos, out var tile) || CanEnter(tile, dir);
    }

    public static bool CanExit(DynamicObject dynamic, Vector2Int dir) {
        MapSystem.Instance.TryGetTile(dynamic.GridPos, out var tile);
        foreach (var obj in tile) {
            if (obj == dynamic) continue;
            if (!obj.CanExit(dir)) return false;
        }
        return true;
    }

    public static bool TryGetDynamic(IReadOnlyList<MapObject> tile, out DynamicObject dynamic) {
        dynamic = tile.FirstOrDefault(obj => !obj.IsStatic) as DynamicObject;
        return dynamic != null;
    }

    public static bool TryGetDynamic(Vector2Int gridPosition, out DynamicObject dynamic) {
        if (MapSystem.Instance.TryGetTile(gridPosition, out var tile)) {
            return TryGetDynamic(tile, out dynamic);
        } else {
            dynamic = null;
            return false;
        }
    }

    public static bool SoftMove(DynamicObject source, Vector2Int dir, out IEnumerator routine) {
        Debug.Log($"Moving {source} in direction {dir}");

        var sourcePos = source.GridPos;
        var targetPos = sourcePos + dir;

        if (CanEnter(targetPos, -dir) && CanExit(source, dir)) {
            routine = MapSystem.Instance.MoveObjectRoutine(source, targetPos);
            return true;
        } else {
            routine = null;
            return false;
        }
    }

    public static bool Push(DynamicObject source, Vector2Int dir, List<IEnumerator> outputList) {
        Debug.Log($"Pushing {source} in direction {dir}");

        var sourcePos = source.GridPos;
        var targetPos = sourcePos + dir;

        if (CanEnter(targetPos, -dir) && CanExit(source, dir)) {
            outputList.Add(MapSystem.Instance.MoveObjectRoutine(source, targetPos));
            return true;
        } else if (TryGetDynamic(targetPos, out var dynamic)) {
            if (dynamic == source) {
                Debug.LogWarning($"Trying to push {source} into itself!");
                return false;
            }
            if (Push(dynamic, dir, outputList)) {
                outputList.Add(MapSystem.Instance.MoveObjectRoutine(source, targetPos));
                return true;
            }
        }

        return false;
    }
}
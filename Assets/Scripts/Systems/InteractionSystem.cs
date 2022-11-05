using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class InteractionSystem {
    public static bool CanEnter(List<MapObject> tile, Vector2Int dir) {
        foreach (var obj in tile) {
            if (!obj.CanEnter(dir)) return false;
        }
        return true;
    }

    public static bool CanEnter(Vector2Int gridPos, Vector2Int dir) {
        return !MapSystem.Instance.TryGetTile(gridPos, out var tile) || CanEnter(tile, dir);
    }

    public static bool TryGetDynamic(List<MapObject> tile, out DynamicObject dynamic) {
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

    public static bool Push(DynamicObject source, Vector2Int dir, out List<IEnumerator> routineList) {
        Debug.Log($"Pushing {source} in direction {dir}");

        var sourcePos = source.GridPos;
        var targetPos = sourcePos + dir;

        if (CanEnter(targetPos, -dir)) {
            routineList = new() { MapSystem.Instance.MoveObjectRoutine(source, targetPos) };
            return true;
        } else if (TryGetDynamic(targetPos, out var dynamic)) {
            if (dynamic == source) {
                Debug.LogError($"Trying to push {source} into itself");
                routineList = null;
                return false;
            }
            if (Push(dynamic, dir, out routineList)) {
                routineList.Add(MapSystem.Instance.MoveObjectRoutine(source, targetPos));
                return true;
            }
        }

        routineList = null;
        return false;
    }
}
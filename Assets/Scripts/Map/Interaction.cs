using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Interaction {
    public static bool CanEnter(IReadOnlyCollection<MapObject> tile, Vector2Int dir) {
        if (tile == null) return true;
        foreach (var obj in tile) {
            if (!obj.CanEnter(dir)) return false;
        }
        return true;
    }

    public static bool CanExit(IReadOnlyCollection<MapObject> tile, DynamicObject dynamic, Vector2Int dir) {
        foreach (var obj in tile) {
            if (obj == dynamic) continue;
            if (!obj.CanExit(dir)) return false;
        }
        return true;
    }

    public static bool TryGetDynamic(IReadOnlyCollection<MapObject> tile, out DynamicObject dynamic) {
        dynamic = tile.FirstOrDefault(obj => !obj.IsStatic) as DynamicObject;
        return dynamic != null;
    }

    public static bool SoftMove(DynamicObject dynamic, Vector2Int dir, out IEnumerator routine) {
        Debug.Log($"Moving {dynamic} in direction {dir}");

        var mapSystem = MapSystem.Instance;

        var sourcePos = dynamic.GridPos;
        var targetPos = sourcePos + dir;

        mapSystem.TryGetTile(sourcePos, out var sourceTile);
        mapSystem.TryGetTile(targetPos, out var targetTile);

        if (CanEnter(targetTile, -dir) && CanExit(sourceTile, dynamic, dir)) {
            routine = MapSystem.Instance.MoveObjectRoutine(dynamic, targetPos);
            return true;
        } else {
            routine = null;
            return false;
        }
    }

    public static IEnumerator PushRoutine(DynamicObject source, Vector2Int dir) {
        Debug.Log($"Pushing {source} in direction {dir}");

        var canPush = PushRecursive(source, dir, out var pushed);
        
        if (canPush) {
            var routines = pushed.Select(obj => MapSystem.Instance.MoveObjectRoutine(obj, obj.GridPos + dir)).ToArray();
            yield return Scheduler.PlayListRoutine(routines);
        }
    }

    static bool PushRecursive(DynamicObject dynamic, Vector2Int dir, out IList<DynamicObject> pushed) {
        var mapSystem = MapSystem.Instance;

        var sourcePos = dynamic.GridPos;
        var targetPos = sourcePos + dir;

        mapSystem.TryGetTile(sourcePos, out var sourceTile);
        mapSystem.TryGetTile(targetPos, out var targetTile);

        if (CanEnter(targetTile, -dir) && CanExit(sourceTile, dynamic, dir)) {
            pushed = new List<DynamicObject>() { dynamic };
            return true;
        } else if (TryGetDynamic(targetTile, out var other)) {
            if (other == dynamic) {
                Debug.LogWarning($"Trying to push {dynamic} into itself!");
                pushed = null;
                return false;
            }
            if (PushRecursive(dynamic, dir, out pushed)) {
                pushed.Add(dynamic);
                return true;
            }
        }

        pushed = null;
        return false;
    }
}
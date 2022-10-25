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
        return MapSystem.instance.TryGetTile(gridPos, out var tile) && CanEnter(tile, dir);
    }

    public static bool TryGetDynamic(List<MapObject> tile, out MapObject dynamic) {
        dynamic = tile.FirstOrDefault(obj => !obj.IsStatic);
        return dynamic != null;
    }

    public static bool TryGetDynamic(Vector2Int gridPosition, out MapObject dynamic) {
        if (MapSystem.instance.TryGetTile(gridPosition, out var tile)) {
            return TryGetDynamic(tile, out dynamic);
        } else {
            dynamic = null;
            return false;
        }
    }

    public static bool Push(MapObject source, Vector2Int dir) {
        var sourcePos = source.GetGridPos();
        var targetPos = sourcePos + dir;

        var isFilled = MapSystem.instance.TryGetTile(targetPos, out var targetTile);
        if (!isFilled) goto Move;

        var canEnter = CanEnter(targetTile, -dir);
        if (canEnter) goto Move;

        if (TryGetDynamic(targetTile, out var dynamic)) {
            var canPush = Push(dynamic, dir);
            if (canPush) goto Move;
        }
        
        return false;

        Move:

        MapSystem.instance.MoveMapObject(source, targetPos);
        return true;
    }
}
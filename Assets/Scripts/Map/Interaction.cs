using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Interaction {
    static MapSystem MapSystem {
        get => _mapSystem != null ? _mapSystem : (_mapSystem = MapSystem.Instance);
    }

    static MapSystem _mapSystem;
    const float DefaultMoveSpeed = 3f;
    const LeanTweenType DefaultEaseType = LeanTweenType.easeInOutQuad;
        
    public static IEnumerator EaseMove(MapObject mapObject, Vector2Int gridPosition, LeanTweenType easeType, float speed) {
        MapSystem.RelocateObject(mapObject, gridPosition);
        var prev = mapObject.transform.position;
        var target = MapSystem.GridToWorld(gridPosition);
        var duration = Vector2.Distance(prev, target) / speed;
        
        LeanTween.move(mapObject.gameObject, target, duration).setEase(easeType);
        yield return Helpers.Wait(duration);
    }

    public static IEnumerator EaseEvent(MapEvent mapEvent, LeanTweenType easeType, float speed, bool staggered = false) {
        var mapObjects = mapEvent.MapObjects;

        var moveActions = mapObjects.Select(obj => EaseMove(obj, obj.GridPos + mapEvent.Direction, easeType, speed)).ToArray();
        var rotateActions = mapObjects.Select(obj => obj.RotateRoutine(mapEvent.Rotation)).ToArray();

        if (staggered) {
            for (var i = 0; i < mapObjects.Count; i++) {
                yield return Scheduler.GroupRoutines(moveActions[i], rotateActions[i]);
            }
        } else {
            yield return Scheduler.GroupRoutines(
                Scheduler.GroupRoutines(moveActions),
                Scheduler.GroupRoutines(rotateActions)
            );
        }
    }

    public static IEnumerator EaseEvent(MapEvent mapEvent, bool staggered = false)
        => EaseEvent(mapEvent, DefaultEaseType, DefaultMoveSpeed, staggered);

    public static void ExecuteEvent(MapEvent mapEvent) {
        foreach (var obj in mapEvent.MapObjects) {
            MapSystem.MoveObjectInstant(obj, obj.GridPos + mapEvent.Direction);
            obj.RotateInstant(mapEvent.Rotation);
        }
    }

    public static bool CheckTile<T>(IEnumerable<MapObject> tile, Func<T, bool> predicate, MapObject exclude) where T : IMapObject {
        return tile.Where(o => o != exclude).OfType<T>().All(predicate);
    }

    public static bool CheckTile<T>(IReadOnlyCollection<MapObject> tile, Func<T, bool> predicate) where T : IMapObject {
        return tile.OfType<T>().All(predicate);
    }

    public static bool SoftMove(MapObject mapObject, Vector2Int dir, out MapEvent mapEvent) {
        var targetPos = mapObject.GridPos + dir;

        if (MapSystem.TryGetTile(targetPos, out var tile)) {
            var sourceTile = MapSystem.GetTile(mapObject.GridPos);

            if (!CheckTile(sourceTile, (ICanExitHandler o) => o.CanExit(dir), mapObject)
                || !CheckTile(tile, (ICanEnterHandler o) => o.CanEnter(dir))) {
                mapEvent = default;
                return false;
            }
        }
        mapEvent = new MapEvent(mapObject, dir);
        return true;
    }

    public static bool Push(MapObject mapObject, Vector2Int dir, out MapEvent mapEvent){
        mapEvent = new MapEvent(mapObject, dir);

        // Check exit
        var sourceTile = MapSystem.GetTile(mapObject.GridPos);
        var canExit = CheckTile(sourceTile, (ICanExitHandler o) => o.CanExit(dir), mapObject);
        if (!canExit) return false;
        
        var targetTileFilled = MapSystem.TryGetTile(mapObject.GridPos + dir, out var targetTile);
        if (!targetTileFilled) return true;

        // Check enter
        var blockages = targetTile.OfType<ICanEnterHandler>().Where(o => !o.CanEnter(-dir)).ToArray();
        if (blockages.Length == 0) return true;
        if (blockages.Any(o => !o.Pushable)) return false;

        // Push next
        if (Push(blockages[0].Object, dir, out var _)) {
            // If we can push one, push all
            mapEvent.MapObjects.AddRange(blockages.Select(o => o.Object));
            return true;
        } else {
            return false;
        }
    }
}

public class MapEvent {
    public readonly List<MapObject> MapObjects;
    public readonly Vector2Int Direction;
    public readonly int Rotation;
    
    public MapEvent(IEnumerable<IMapObject> mapObjects, Vector2Int direction, int rotation = 0) {
        var list = mapObjects.Select(o => o.Object).ToList();
        if (mapObjects == null || list.Count == 0) {
            throw new ArgumentException("MapObjects cannot be null or empty", nameof(mapObjects));
        }
        MapObjects = list;
        Direction = direction;
        Rotation = rotation;
    }
    
    public MapEvent(IMapObject mapObject, Vector2Int direction, int rotation = 0) {
        MapObjects = new List<MapObject> { mapObject.Object };
        Direction = direction;
        Rotation = rotation;
    }
}
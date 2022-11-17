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

    public static IEnumerator EaseAction(MapAction action, LeanTweenType easeType, float speed, bool staggered = false) {
        var mapObjects = action.MapObjects;

        var moveActions = mapObjects.Select(obj => EaseMove(obj, obj.GridPos + action.Direction, easeType, speed)).ToArray();
        var rotateActions = mapObjects.Select(obj => obj.RotateRoutine(action.Rotation)).ToArray();

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

    public static IEnumerator EaseAction(MapAction action, bool staggered = false)
        => EaseAction(action, DefaultEaseType, DefaultMoveSpeed, staggered);

    public static void ExecuteAction(MapAction action) {
        foreach (var obj in action.MapObjects) {
            MapSystem.MoveObjectInstant(obj, obj.GridPos + action.Direction);
            obj.RotateInstant(action.Rotation);
        }
    }

    public static bool CheckTile<T>(IEnumerable<MapObject> tile, Func<T, bool> predicate, MapObject exclude) where T : IMapObject {
        return tile.Where(o => o != exclude).OfType<T>().All(predicate);
    }

    public static bool CheckTile<T>(IReadOnlyCollection<MapObject> tile, Func<T, bool> predicate) where T : IMapObject {
        return tile.OfType<T>().All(predicate);
    }

    public static bool SoftMove(MapObject mapObject, Vector2Int dir, out MapAction action) {
        var targetPos = mapObject.GridPos + dir;

        if (MapSystem.TryGetTile(targetPos, out var tile)) {
            var sourceTile = MapSystem.GetTile(mapObject.GridPos);

            if (!CheckTile(sourceTile, (ICanExitHandler o) => o.CanExit(dir), mapObject)
                || !CheckTile(tile, (ICanEnterHandler o) => o.CanEnter(dir))) {
                action = default;
                return false;
            }
        }
        action = new MapAction(mapObject, dir);
        return true;
    }

    public static bool Push(MapObject mapObject, Vector2Int dir, out MapAction action) {
        action = new MapAction(mapObject, dir);

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
            action.MapObjects.AddRange(blockages.Select(o => o.Object));
            return true;
        } else {
            return false;
        }
    }

    public struct MapAction {
        public readonly List<MapObject> MapObjects;
        public readonly Vector2Int Direction;
        public readonly int Rotation;

        public MapAction(IEnumerable<MapObject> mapObjects, Vector2Int direction, int rotation = 0) {
            var list = mapObjects.ToList();
            if (mapObjects == null || list.Count == 0) {
                throw new ArgumentException("MapObjects cannot be null or empty", nameof(mapObjects));
            }
            MapObjects = list;
            Direction = direction;
            Rotation = rotation;
        }

        public MapAction(MapObject mapObject, Vector2Int direction, int rotation = 0) {
            MapObjects = new List<MapObject> { mapObject };
            Direction = direction;
            Rotation = rotation;
        }
    }
}
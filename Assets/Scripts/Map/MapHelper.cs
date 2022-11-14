using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MapHelper {
    static MapSystem MapSystem {
        get => _mapSystem ?? (_mapSystem = MapSystem.Instance);
    }

    static MapSystem _mapSystem;

    public static IEnumerator EaseMove(MapObject mapObject, Vector2Int gridPosition, LeanTweenType easeType, float speed) {
        MapSystem.RelocateObject(mapObject, gridPosition);
        var prev = mapObject.transform.position;
        var target = MapSystem.GridToWorld(gridPosition);
        var duration = Vector2.Distance(prev, target) / speed;
        yield return LeanTween.move(mapObject.gameObject, target, duration).setEase(easeType);
    }

    public static IEnumerator EaseAction(MapAction action, LeanTweenType easeType, float speed, bool staggered = false) {
        var mapObjects = action.MapObjects;

        var moveActions = mapObjects.Select(obj => EaseMove(obj, obj.GridPos + action.Direction, easeType, speed)).ToArray();
        var rotateActions = mapObjects.Select(obj => obj.RotateRoutine(action.Rotation)).ToArray();

        if (staggered) {
            for (int i = 0; i < mapObjects.Length; i++) {
                yield return Scheduler.GroupRoutines(moveActions[i], rotateActions[i]);
            }
        } else {
            yield return Scheduler.GroupRoutines(
                Scheduler.GroupRoutines(moveActions),
                Scheduler.GroupRoutines(rotateActions)
            );
        }
    }

    public static void ExecuteAction(MapAction action) {
        foreach (var obj in action.MapObjects) {
            MapSystem.MoveObjectInstant(obj, obj.GridPos + action.Direction);
            obj.RotateInstant(action.Rotation);
        }
    }

    public static bool CheckTile<T>(IReadOnlyCollection<MapObject> tile, Func<T, bool> predicate, MapObject exclude) where T : IMapObject {
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
        action = new(mapObject, dir);
        return true;
    }

    public static bool Push(MapObject mapObject, Vector2Int dir, out MapAction action) {
        var sourcePos = mapObject.GridPos;
        var targetPos = sourcePos + dir;
        
        action = new(mapObject, dir);

        // Quick check
        if (SoftMove(mapObject, dir, out action)) {
            return true;
        } else {
            var sourceTile = MapSystem.GetTile(sourcePos).Where(o => o != mapObject).ToArray();
            var targetFilled = MapSystem.TryGetTile(targetPos, out var targetTile);

            // If target is not filled, onexit from the quick check is enough
            if (targetFilled) {
                var obstacles = targetTile.OfType<IObstacle>().ToArray();
                if (obstacles.Any(o => !o.Pushable)) return false;

                foreach (var obstacle in obstacles) {
                    if (Push(obstacle.Object, dir, out var obstacleAction)) {
                        action += obstacleAction;
                    } else {
                        return false;
                    }
                }
                return true;
            }
        }
        return false;
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
            MapObjects = new() { mapObject };
            Direction = direction;
            Rotation = rotation;
        }

        public static MapAction operator +(MapAction a, MapAction b) {
            if (a.Direction != b.Direction) {
                throw new ArgumentException("Cannot combine actions with different directions");
            }
            if (a.Rotation != b.Rotation) {
                throw new ArgumentException("Cannot combine actions with different rotations");
            }

            var list = a.MapObjects.Concat(b.MapObjects).ToList();
            return new(list, a.Direction, a.Rotation);
        }
    }
}
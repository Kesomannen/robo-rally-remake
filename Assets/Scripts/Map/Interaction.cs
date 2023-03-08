using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Interaction {
    static MapSystem MapSystem {
        get => _mapSystem != null ? _mapSystem : _mapSystem = MapSystem.Instance;
    }

    static MapSystem _mapSystem;
    const float DefaultMoveSpeed = 2.5f;
    const LeanTweenType DefaultEaseType = LeanTweenType.easeInOutQuad;

    static IEnumerator EaseMove(MapObject mapObject, Vector2Int gridPosition, LeanTweenType easeType, float speed) {
        var prev = mapObject.transform.position;
        var target = MapSystem.GridToWorld(gridPosition);
        var duration = Vector2.Distance(prev, target) / speed;
        
        LeanTween.move(mapObject.gameObject, target, duration).setEase(easeType);
        yield return CoroutineUtils.Wait(duration);
        MapSystem.RelocateObject(mapObject, gridPosition);
    }

    public static IEnumerator EaseEvent(MapEvent mapEvent, LeanTweenType easeType, float speed, bool staggered = false) {
        var mapObjects = mapEvent.MapObjects;

        var moveActions = mapObjects.Select(obj => EaseMove(obj, obj.GridPos + mapEvent.Direction, easeType, speed)).ToArray();
        var rotateActions = mapObjects.Select(obj => obj.RotateRoutine(mapEvent.Rotation)).ToArray();

        var mono = TaskScheduler.Instance;
        if (staggered) {
            for (var i = 0; i < mapObjects.Count; i++) {
                yield return mono.RunRoutines(moveActions[i], rotateActions[i]);
            }
        } else {
            yield return mono.RunRoutines(moveActions.Concat(rotateActions).ToArray());
        }
    }

    public static IEnumerator EaseEvent(MapEvent mapEvent, bool staggered = false)
        => EaseEvent(mapEvent, DefaultEaseType, DefaultMoveSpeed, staggered);

    static bool CheckTile<T>(IEnumerable<MapObject> tile, Func<T, bool> predicate, MapObject exclude) where T : IMapObject{
        return exclude == null ? CheckTile(tile, predicate) : tile.Where(o => o != exclude).OfType<T>().All(predicate);
    }

    static bool CheckTile<T>(IEnumerable<MapObject> tile, Func<T, bool> predicate) where T : IMapObject {
        return tile.OfType<T>().All(predicate);
    }

    public static bool CanMove(Vector2Int source, Vector2Int dir, MapObject except = null){
        var targetPos = source + dir;
        var canEnter = !MapSystem.TryGetTile(targetPos, out var tile) 
                       || CheckTile(tile, (ICanEnterHandler o) => o.CanEnter(-dir));

        var canExit = !MapSystem.TryGetTile(source, out var sourceTile) 
                      || CheckTile(sourceTile, (ICanExitHandler o) => o.CanExit(dir), except);

        return canEnter && canExit;
    }
    
    public static bool SoftMove(MapObject mapObject, Vector2Int dir, out MapEvent mapEvent) {
        if (CanMove(mapObject.GridPos, dir, mapObject)){
            mapEvent = new MapEvent(mapObject, dir);
            return true;
        }
        mapEvent = null;
        return false;
    }

    public static bool Push(MapObject mapObject, Vector2Int dir, out MapEvent mapEvent, IReadOnlyList<Type> ignoredTypes = null) {
        mapEvent = null;

        // Check exit
        var sourceTile = MapSystem.GetTile(mapObject.GridPos).Where(IsConsidered);
        var canExit = CheckTile(sourceTile, (ICanExitHandler o) => o.CanExit(dir), mapObject);
        if (!canExit) return false;
        
        mapEvent = new MapEvent(mapObject, dir);
        
        var targetTileFilled = MapSystem.TryGetTile(mapObject.GridPos + dir, out var targetTile);
        if (!targetTileFilled) return true;
        targetTile = targetTile.Where(IsConsidered).ToArray();

        // Check enter
        var blockages = targetTile.OfType<ICanEnterHandler>().Where(o => !o.CanEnter(-dir)).ToArray();
        if (blockages.Length == 0) return true;
        if (blockages.Any(o => !o.Movable)) return false;

        // Push next
        if (!Push(blockages[0].Object, dir, out _)) return false;
        // If we can push one, push all
        mapEvent.MapObjects.AddRange(blockages.Select(o => o.Object));
        return true;
        
        bool IsConsidered(IMapObject obj) => ignoredTypes == null || !ignoredTypes.Contains(obj.GetType());
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
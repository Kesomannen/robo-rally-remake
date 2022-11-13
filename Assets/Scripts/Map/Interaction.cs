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

    public static bool CanExit(IReadOnlyCollection<MapObject> tile, PlayerModel playerModel, Vector2Int dir) {
        foreach (var obj in tile) {
            if (obj == playerModel) continue;
            if (!obj.CanExit(dir)) return false;
        }
        return true;
    }

    public static bool TryGetPlayerModel(IReadOnlyCollection<MapObject> tile, out PlayerModel playerModel) {
        playerModel = tile.FirstOrDefault(obj => !obj.IsStatic) as PlayerModel;
        return playerModel != null;
    }

    public static bool SoftMove(PlayerModel playerModel, Vector2Int dir, out IEnumerator routine) {
        Debug.Log($"Moving {playerModel} in direction {dir}");

        var mapSystem = MapSystem.Instance;

        var playerPos = playerModel.GridPos;
        var targetPos = playerPos + dir;

        mapSystem.TryGetTile(playerPos, out var sourceTile);
        mapSystem.TryGetTile(targetPos, out var targetTile);

        if (CanEnter(targetTile, -dir) && CanExit(sourceTile, playerModel, dir)) {
            routine = MapSystem.Instance.MoveObjectRoutine(playerModel, targetPos);
            return true;
        } else {
            routine = null;
            return false;
        }
    }

    public static IEnumerator PushRoutine(PlayerModel playerModel, Vector2Int dir) {
        Debug.Log($"Pushing {playerModel} in direction {dir}");

        var canPush = PushRecursive(playerModel, dir, out var pushed);
        
        if (canPush) {
            var routines = pushed.Select(obj => MapSystem.Instance.MoveObjectRoutine(obj, obj.GridPos + dir)).ToArray();
            yield return Scheduler.PlayListRoutine(routines);
        }
    }

    static bool PushRecursive(PlayerModel playerModel, Vector2Int dir, out IList<PlayerModel> pushed) {
        var mapSystem = MapSystem.Instance;

        var playerPos = playerModel.GridPos;
        var targetPos = playerPos + dir;

        mapSystem.TryGetTile(playerPos, out var sourceTile);
        mapSystem.TryGetTile(targetPos, out var targetTile);

        if (CanEnter(targetTile, -dir) && CanExit(sourceTile, playerModel, dir)) {
            pushed = new List<PlayerModel>() { playerModel };
            return true;
        } else if (TryGetPlayerModel(targetTile, out var other)) {
            if (other == playerModel) {
                Debug.LogWarning($"Trying to push {playerModel} into itself!");
                pushed = null;
                return false;
            } else if (PushRecursive(playerModel, dir, out pushed)) {
                pushed.Add(playerModel);
                return true;
            }
        }

        pushed = null;
        return false;
    }
}
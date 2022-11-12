using System.Collections;
using UnityEngine;

public class RebootToken : StaticObject {
    [SerializeField] Vector2Int _direction;
    [SerializeField] bool _isSpawnPoint;

    public bool IsSpawnPoint => _isSpawnPoint;

    public IEnumerator RespawnRoutine(PlayerModel player) {
        MapSystem.Instance.TryGetTile(GridPos, out var tile);
        if (Interaction.TryGetDynamic(tile, out var dynamic)) {
            yield return Interaction.PushRoutine(dynamic, RotateAsObject(_direction));
        }
        MapSystem.Instance.MoveObjectInstant(player, GridPos);
    }
}
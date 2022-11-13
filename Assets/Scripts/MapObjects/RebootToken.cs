using System.Collections;
using UnityEngine;

public class RebootToken : MapObject {
    [SerializeField] Vector2Int _direction;
    [SerializeField] bool _isSpawnPoint;

    public bool IsSpawnPoint => _isSpawnPoint;

    public IEnumerator RespawnRoutine(PlayerModel player) {
        MapSystem.Instance.TryGetTile(GridPos, out var tile);
        if (Interaction.TryGetPlayerModel(tile, out var dynamic)) {
            yield return Interaction.PushRoutine(dynamic, Rotator.Rotate(_direction));
        }
        MapSystem.Instance.MoveObject(player, GridPos);
    }
}
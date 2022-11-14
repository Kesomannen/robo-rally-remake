using System.Collections;
using UnityEngine;

public class RebootToken : MapObject {
    [SerializeField] Vector2Int _direction;
    [SerializeField] bool _isSpawnPoint;

    public bool IsSpawnPoint => _isSpawnPoint;

    public IEnumerator RespawnRoutine(PlayerModel playerModel) {
        MapSystem.Instance.TryGetTile(GridPos, out var tile);
        if (MapHelper.TryGetPlayerModel(tile, out var dynamic)) {
            yield return MapHelper.PushRoutine(dynamic, Rotator.Rotate(_direction));
        }
        MapSystem.Instance.MoveObject(obj, GridPos);
    }
}
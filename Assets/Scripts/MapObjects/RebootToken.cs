using System.Collections;
using System.Linq;
using UnityEngine;

public class RebootToken : MapObject {
    [SerializeField] Vector2Int _direction;
    [SerializeField] bool _isSpawnPoint;

    Vector2Int _startPos;

    public bool IsSpawnPoint => _isSpawnPoint;

    void Start() {
        _startPos = GridPos;
    }

    public override void Fall(IBoard board) {
        MapSystem.Instance.MoveObjectInstant(Object, _startPos);
    }

    public IEnumerator RespawnRoutine(PlayerModel playerModel) {
        var tile = MapSystem.Instance.GetTile(GridPos);
        var obstructions = tile.OfType<ICanEnterHandler>();
        foreach (var obj in obstructions) {
            if (MapHelper.Push(obj.Object, _direction, out var moveAction)) {
                yield return MapHelper.EaseAction(moveAction);
            } else {
                Debug.LogWarning($"RebootToken is obstructed!", this);
            }
        }
        // TODO: Allow the player to choose direction
        MapSystem.Instance.MoveObjectInstant(playerModel, GridPos);
    }
}
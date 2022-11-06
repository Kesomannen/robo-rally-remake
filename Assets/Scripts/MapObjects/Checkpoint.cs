using UnityEngine;

public class Checkpoint : StaticObject {
    [SerializeField] int _index;
    [SerializeField] bool _isSpawnPoint;

    public int Index => _index;
    public bool IsSpawnPoint => _isSpawnPoint;

    public override bool CanEnter(Vector2Int dir) => true;

    public override void OnEnter(DynamicObject dynamic) {
        base.OnEnter(dynamic);
        
        if (_isSpawnPoint) return;
        if (dynamic is PlayerModel plrModel) {
            var player = plrModel.Owner;
            var current = player.CurrentCheckpoint;

            if (current.Value.Index == _index - 1) {
                current.Value = this;
            }
        }
    }
}
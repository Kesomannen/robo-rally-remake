using UnityEngine;

public class Checkpoint : BoardElement<Checkpoint> {
    [SerializeField] int _index;
    [SerializeField] bool _isSpawnPoint;

    public int Index => _index;
    public bool IsSpawnPoint => _isSpawnPoint;

    protected override void OnActivate() {
        if (_isSpawnPoint) return;
        if (CurrentDynamic is PlayerModel plrModel) {
            var player = plrModel.Owner;
            var current = player.CurrentCheckpoint;

            if (current.Value.Index == _index - 1) {
                current.Value = this;
            }
        }
    }
}
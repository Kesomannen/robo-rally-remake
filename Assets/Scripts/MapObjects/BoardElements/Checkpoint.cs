using UnityEngine;

public class Checkpoint : BoardElement<Checkpoint, IPlayer> {
    [SerializeField] int _index;

    protected override void Activate(IPlayer[] targets) {
        foreach (var playerModel in targets) {
            var player = playerModel.Owner;
            var current = player.CurrentCheckpoint;

            if (current.Value == _index - 1) {
                current.Value = _index;
            }
        }
    }
}
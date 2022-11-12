using UnityEngine;

public class Checkpoint : BoardElement<Checkpoint> {
    [SerializeField] int _index;

    public int Index => _index;

    protected override void Activate(DynamicObject dynamic) {
        if (dynamic is PlayerModel plrModel) {
            var player = plrModel.Owner;
            var current = player.CurrentCheckpoint;

            if (current.Value == _index - 1) {
                current.Value = _index;
            }
        }
    }
}
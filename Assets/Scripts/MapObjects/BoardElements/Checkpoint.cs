using System;
using UnityEngine;

public class Checkpoint : BoardElement<Checkpoint, IPlayer>, ITooltipable {
    [SerializeField] int _index;

    public event Action<Player> OnPlayerReached;

    public string Header => $"Checkpoint {_index}";
    public string Description {
        get {
            var current = PlayerSystem.LocalPlayer.CurrentCheckpoint.Value;
            if (current == _index) return "This is your current checkpoint.";
            if (current + 1 == _index) return "This is the next checkpoint.";
            if (current < _index) return "This is not the next checkpoint.";
            return current > _index ? "You have already passed this checkpoint." 
                : "You have not yet reached this checkpoint.";
        }
    }

    protected override void Activate(IPlayer[] targets) {
        foreach (var playerModel in targets) {
            var player = playerModel.Owner;
            var current = player.CurrentCheckpoint;

            if (current.Value != _index - 1) continue;
            current.Value = _index;
            OnPlayerReached?.Invoke(player);
        }
    }
}
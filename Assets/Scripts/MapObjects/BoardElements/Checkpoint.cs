using System;
using TMPro;
using UnityEngine;

public class Checkpoint : BoardElement<Checkpoint, IPlayer>, ITooltipable {
    [SerializeField] int _index;
    [SerializeField] TMP_Text _indexText;

    public event Action<Player> OnPlayerReached;

    public int Index => _index;
    
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

    protected override void Awake() {
        base.Awake();
        _indexText.text = _index.ToString();
    }

    protected override void Activate(IPlayer[] targets) {
        foreach (var playerModel in targets) {
            var player = playerModel.Owner;
            var current = player.CurrentCheckpoint;

            if (current.Value != _index - 1) continue;
            AddActivation();
            current.Value = _index;
            OnPlayerReached?.Invoke(player);
            
            Log.Instance.CheckpointMessage(player, _index);
        }
    }
}
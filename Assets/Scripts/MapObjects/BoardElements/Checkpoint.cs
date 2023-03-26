using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class Checkpoint : BoardElement<Checkpoint, IPlayer>, ITooltipable, ITriggerAwake {
    [SerializeField] int _index;
    [SerializeField] TMP_Text _indexText;
    [SerializeField] ParticleSystem _particle;
    [SerializeField] SoundEffect _sound;

    public event Action<Player> PlayerReached;

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

    public void TriggerAwake() => Awake();
    protected override void Awake() {
        base.Awake();
        _indexText.text = _index.ToString();
    }

    protected override void Activate(IPlayer[] targets) {
        foreach (var playerModel in targets) {
            var player = playerModel.Owner;
            var current = player.CurrentCheckpoint;

            if (current.Value != _index - 1) continue;
            TaskScheduler.PushRoutine(Routine());
            AddActivation();
            
            IEnumerator Routine() {
                current.Value = _index;
                PlayerReached?.Invoke(player);
                Log.Message($"{Log.PlayerString(player)} reached {Log.CheckpointString(Index)}");
                
                _particle.Play();
                _sound.Play();
                yield return CoroutineUtils.Wait(_particle.main.duration);
            }
        }
    }
}
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class CheckpointSystem : Singleton<CheckpointSystem> {
    static Checkpoint[] _checkpoints;
    
    public static event Action<Player> PlayerWon;

    protected override void Awake() {
        base.Awake();
        MapSystem.MapLoaded += OnMapLoaded;
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        MapSystem.MapLoaded -= OnMapLoaded;
    }

    static void OnMapLoaded() {
        _checkpoints = MapSystem.GetByType<Checkpoint>().ToArray();
        if (_checkpoints.Length == 0) {
            Debug.LogError("No checkpoints found in the scene!");
            return;
        }
        _checkpoints.OrderBy(c => c.Index).Last().PlayerReached += p => TaskScheduler.PushRoutine(OnPlayerReachedLast(p));
    }

    static IEnumerator OnPlayerReachedLast(Player player) {
        PlayerWon?.Invoke(player);
        yield break;
    }
}
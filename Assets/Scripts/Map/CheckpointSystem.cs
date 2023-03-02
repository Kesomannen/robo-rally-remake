using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class CheckpointSystem : Singleton<CheckpointSystem> {
    static Checkpoint[] _checkpoints;

    protected override void Awake() {
        base.Awake();
        MapSystem.OnMapLoaded += OnMapLoaded;
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        MapSystem.OnMapLoaded -= OnMapLoaded;
    }

    static void OnMapLoaded() {
        _checkpoints = MapSystem.GetByType<Checkpoint>().ToArray();
        if (_checkpoints.Length == 0) {
            Debug.LogError("No checkpoints found in the scene!");
            return;
        }
        _checkpoints.OrderBy(c => c.Index).Last().OnPlayerReached += p => TaskScheduler.PushRoutine(OnPlayerReachedLast(p));
    }

    static IEnumerator OnPlayerReachedLast(Player player) {
        PhaseSystem.StopPhaseSystem();
        Debug.Log($"{player} won the game!");
        // TODO: Show win screen
        yield return CoroutineUtils.Wait(3);
        NetworkSystem.ReturnToLobby();
    }
}
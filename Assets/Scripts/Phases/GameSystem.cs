using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSystem : Singleton<GameSystem> {
    [SerializeField] PlayerModel _playerModelPrefab;
    
    static bool _isPlaying;
    static GameOptions _options;

    public static ObservableField<Phase> CurrentPhase { get; } = new();

    protected override void OnDestroy() {
        base.OnDestroy();
        StopGame();
    }

    public void StartGame(GameOptions options) {
        if (_isPlaying) return;
        _options = options;
        StartCoroutine(PhaseSystemRoutine());
    }

    public struct GameOptions {
        public bool ShopEnabled;
        public bool DoSetupPhase;
        public IReadOnlyDictionary<Player, RebootToken> PlayerSpawns;
    }

    public static void StopGame() {
        _isPlaying = false;
    }

    IEnumerator PhaseSystemRoutine() {
        _isPlaying = true;
        
        if (_options.DoSetupPhase) {
            yield return DoPhaseRoutine(SetupPhase.Instance.DoPhase(), Phase.Setup);   
        } else {
            foreach (var player in PlayerSystem.Players) {
                player.CreateModel(_playerModelPrefab, _options.PlayerSpawns[player]);
            }
        }

        while (_isPlaying) {
            if (_options.ShopEnabled) {
                yield return DoPhaseRoutine(ShopPhase.Instance.DoPhase(), Phase.Shop);   
            }
            yield return DoPhaseRoutine(ProgrammingPhase.Instance.DoPhase(), Phase.Programming);
            yield return DoPhaseRoutine(ExecutionPhase.Instance.DoPhase(), Phase.Execution);
        }
        
        IEnumerator DoPhaseRoutine(IEnumerator routine, Phase phase) {
            yield return TaskScheduler.WaitUntilClear();
            
            yield return NetworkSystem.Instance.SyncPlayers();
            CurrentPhase.Value = phase;
            yield return routine;
        }
    }
}

public enum Phase {
    Programming,
    Execution,
    Shop,
    Setup
}
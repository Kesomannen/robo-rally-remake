using System.Collections;
using UnityEngine;

public class PhaseSystem : Singleton<PhaseSystem> {
    static bool _isRunning;

    public static ObservableField<Phase> Current { get; } = new();

    protected override void OnDestroy() {
        base.OnDestroy();
        StopPhaseSystem();
    }

    public void StartPhaseRoutine() {
        if (_isRunning) return;
        StartCoroutine(PhaseSystemRoutine());
    }

    public static void StopPhaseSystem() {
        _isRunning = false;
    }

    static IEnumerator PhaseSystemRoutine() {
        _isRunning = true;
        yield return DoPhaseRoutine(SetupPhase.Instance.DoPhase(), Phase.Setup);
        while (true) {
            if (PlayerSystem.EnergyEnabled) {
                yield return DoPhaseRoutine(ShopPhase.Instance.DoPhase(), Phase.Shop);   
            }
            yield return DoPhaseRoutine(ProgrammingPhase.Instance.DoPhase(), Phase.Programming);
            yield return DoPhaseRoutine(ExecutionPhase.Instance.DoPhase(), Phase.Execution);
        }
        
        IEnumerator DoPhaseRoutine(IEnumerator routine, Phase phase) {
            yield return TaskScheduler.WaitUntilClear();
            yield return NetworkSystem.Instance.SyncPlayers();
            Current.Value = phase;
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
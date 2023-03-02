using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PhaseSystem : Singleton<PhaseSystem> {
    static bool _isRunning;

    public static ObservableField<Phase> Current { get; } = new();

    public static void StartPhaseSystem() {
        Instance.StartCoroutine(PhaseSystemRoutine());
    }

    public static void StopPhaseSystem() {
        _isRunning = false;
    }

    static IEnumerator PhaseSystemRoutine() {
        _isRunning = true;
        while (true) {
            yield return DoPhaseRoutine(ShopPhase.Instance.DoPhase(), Phase.Shop);
            if (!_isRunning) yield break;   
            yield return DoPhaseRoutine(ProgrammingPhase.DoPhase(), Phase.Programming);
            if (!_isRunning) yield break;
            yield return DoPhaseRoutine(ExecutionPhase.DoPhase(), Phase.Execution);
            if (!_isRunning) yield break;
        }
        
        IEnumerator DoPhaseRoutine(IEnumerator routine, Phase phase) {
            yield return NetworkSystem.Instance.SyncPlayers();
            yield return TaskScheduler.WaitUntilClear();
            Current.Value = phase;
            yield return routine;
        }
    }
}

public enum Phase {
    Programming,
    Execution,
    Shop,
}
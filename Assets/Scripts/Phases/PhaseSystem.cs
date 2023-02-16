using System.Collections;
using Unity.Netcode;

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
            if (NetworkManager.Singleton != null) {
                yield return DoPhaseRoutine(ShopPhase.Instance.DoPhase(), Phase.Shop);
                if (!_isRunning) yield break;   
            }
            yield return DoPhaseRoutine(ProgrammingPhase.DoPhase(), Phase.Programming);
            if (!_isRunning) yield break;
            yield return DoPhaseRoutine(ExecutionPhase.DoPhase(), Phase.Execution);
            if (!_isRunning) yield break;
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
}
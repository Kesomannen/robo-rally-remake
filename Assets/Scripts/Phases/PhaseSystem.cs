using System.Collections;

public class PhaseSystem : Singleton<PhaseSystem> {
    static bool _isRunning;

    public static Phase CurrentPhase { get; private set; }

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
            yield return DoPhaseRoutine(ProgrammingPhase.Instance.DoPhase(), Phase.Programming);
            if (!_isRunning) yield break;
            yield return DoPhaseRoutine(ExecutionPhase.Instance.DoPhase(), Phase.Execution);
            if (!_isRunning) yield break;
        }
        
        IEnumerator DoPhaseRoutine(IEnumerator routine, Phase phase) {
            CurrentPhase = phase;
            yield return routine;
            yield return TaskScheduler.WaitUntilClear();
        }
    }
}

public enum Phase {
    Programming,
    Execution,
    Shop,
}
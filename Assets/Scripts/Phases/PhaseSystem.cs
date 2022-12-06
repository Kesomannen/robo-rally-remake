using System.Collections;

public class PhaseSystem : Singleton<PhaseSystem> {
    bool _isRunning;

    public static Phase CurrentPhase { get; private set; }
    
    const float PhaseDelay = 1f;
    
    void StartPhaseSystem() {
        _isRunning = true;
        StartCoroutine(PhaseSystemRoutine());
    }

    IEnumerator PhaseSystemRoutine() {
        while (_isRunning){
            yield return DoPhaseRoutine(ShopPhase.DoPhaseRoutine(), Phase.Shop);
            yield return DoPhaseRoutine(ProgrammingPhase.DoPhaseRoutine(), Phase.Programming);
            yield return DoPhaseRoutine(ExecutionPhase.DoPhaseRoutine(), Phase.Execution);
        }
        
        IEnumerator DoPhaseRoutine(IEnumerator routine, Phase phase) {
            CurrentPhase = phase;
            yield return routine;
            yield return CoroutineUtils.Wait(PhaseDelay);
        }
    }

    void Start() {
        StartPhaseSystem();
    }
}

public enum Phase {
    Programming,
    Execution,
    Shop,
}
using System.Collections;
using UnityEngine;

public class PhaseSystem : Singleton<PhaseSystem> {
    bool _isRunning;

    public static Phase CurrentPhase { get; private set; }
    
    void StartPhaseSystem() {
        _isRunning = true;
        StartCoroutine(PhaseSystemRoutine());
    }

    IEnumerator PhaseSystemRoutine() {
        while (_isRunning){
            CurrentPhase = Phase.Shop;
            yield return ShopPhase.DoPhaseRoutine();
            CurrentPhase = Phase.Programming;
            yield return ProgrammingPhase.DoPhaseRoutine();
            CurrentPhase = Phase.Execution;
            yield return ExecutionPhase.DoPhaseRoutine();
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
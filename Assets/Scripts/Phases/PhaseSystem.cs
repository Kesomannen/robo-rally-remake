using System.Collections;
using UnityEngine;

public class PhaseSystem : Singleton<PhaseSystem> {
    bool _isRunning;

    void StartPhaseSystem() {
        _isRunning = true;
        StartCoroutine(PhaseSystemRoutine());
    }

    IEnumerator PhaseSystemRoutine() {
        while (_isRunning) {
            yield return ProgrammingPhase.DoPhaseRoutine();
            yield return ExecutionPhase.DoPhaseRoutine();
        }
    }

    void Start() {
        StartPhaseSystem();
    }
}
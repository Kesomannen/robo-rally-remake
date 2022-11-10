using System.Collections;
using UnityEngine;

public class PhaseSystem : Singleton<PhaseSystem> {
    bool _isRunning;

    public bool IsRunning {
        get => _isRunning;
        set {
            if (_isRunning == value) return;

            _isRunning = value;
            if (value) {
                StartPhaseSystem();
            }
        }
    }

    public void StartPhaseSystem() {
        IsRunning = true;
        StartCoroutine(PhaseSystemRoutine());
    }

    IEnumerator PhaseSystemRoutine() {
        while (_isRunning) {
            yield return ProgrammingPhase.DoPhaseRoutine();
            yield return ExecutionPhase.DoPhaseRoutine();
        }
    }

    void Start() {
        IsRunning = true;
    }
}
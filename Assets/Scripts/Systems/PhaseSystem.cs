using System.Collections;
using UnityEngine;

public class PhaseSystem : Singleton<PhaseSystem> {
    IEnumerator Start() {
        while (true) {
            yield return ProgrammingPhase.Instance.DoPhase();
            yield return ExecutionPhase.Instance.DoPhase();
        }
    }
}
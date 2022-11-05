using System.Collections;
using UnityEngine;

public class PhaseSystem : MonoBehaviour {
    IEnumerator Start() {
        while (true) {
            yield return Helpers.Wait(1);
            yield return ProgrammingPhase.DoPhaseRoutine();
            yield return Helpers.Wait(1);
            yield return ExecutionPhase.DoPhaseRoutine();
        }
    }
}
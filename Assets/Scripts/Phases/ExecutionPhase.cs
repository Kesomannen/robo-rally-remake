using System.Collections;
using Unity.Netcode;
using UnityEngine;

#pragma warning disable 0067

public class ExecutionPhase : NetworkSingleton<ProgrammingPhase> {
    public static int CurrentRegister { get; private set; }
    public static Player CurrentPlayer { get; private set; }

    public const int RegisterCount = 5;

    public static IEnumerator DoPhaseRoutine() {
        UIManager.Instance.CurrentState = UIState.Map;
        for (CurrentRegister = 0; CurrentRegister < RegisterCount; CurrentRegister++) {
            foreach (var player in PlayerManager.OrderPlayers()) {
                CurrentPlayer = player;
                var card = player.Registers[CurrentRegister];
                var routine = card.ExecuteRoutine(player, CurrentRegister);

                Scheduler.AddRoutine(routine);
                yield return Scheduler.WaitUntilClearRoutine();
            }
            yield return Conveyor.ActivateRoutine();
        }
    }
}
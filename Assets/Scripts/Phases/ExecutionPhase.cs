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
            foreach (var player in PlayerManager.GetOrderedPlayers()) {
                CurrentPlayer = player;
                var card = player.Registers[CurrentRegister];
                Debug.Log($"Executing {card.Name} for {player}");
                var routine = card.ExecuteRoutine(player, CurrentRegister);

                Scheduler.AddRoutine(routine);
                yield return Scheduler.WaitUntilClearRoutine();
            }
        }
    }
}
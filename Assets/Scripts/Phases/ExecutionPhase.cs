using System.Collections;
using Unity.Netcode;
using UnityEngine;

#pragma warning disable 0067

public class ExecutionPhase : NetworkSingleton<ExecutionPhase> {
    public static int CurrentRegister { get; private set; }
    public static Player CurrentPlayer { get; private set; }

    public const int RegisterCount = 5;

    public static IEnumerator DoPhaseRoutine() {
        UIManager.Instance.CurrentState = UIState.Map;

        yield return ExecuteRegisters();

        // Board elements
        yield return Conveyor.ActivateRoutine();

        DiscardRegisters();
    }

    static IEnumerator ExecuteRegisters() {
        for (CurrentRegister = 0; CurrentRegister < RegisterCount; CurrentRegister++) {
            var orderedPlayers = PlayerManager.GetOrderedPlayers();
            foreach (var player in orderedPlayers) {
                CurrentPlayer = player;
                var card = player.Registers[CurrentRegister];
                Debug.Log($"Executing {card.Name} for {player}");
                var routine = card.ExecuteRoutine(player, CurrentRegister);

                Scheduler.AddRoutine(routine);
                yield return Scheduler.WaitUntilClearRoutine();
            }
        }
    }

    static void DiscardRegisters() {
        foreach (var player in PlayerManager.Players) {
            for (int i = 0; i < player.Registers.Length; i++) {
                player.DiscardPile.AddCard(player.Registers[i], CardPlacement.Top);
                player.Registers[i] = null;
            }
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ExecutionPhase : NetworkSingleton<ExecutionPhase> {
    public static int CurrentRegister { get; private set; }
    public static ExecutionSubPhase CurrentSubPhase { get; private set; }
    public static Player CurrentPlayer { get; private set; }

    public const int RegisterCount = 5;
    const float SubPhaseDelay = 0;
    const float StepDelay = 1f;

    public static event Action OnPhaseStart, OnPhaseEnd, OnExecutionComplete;
    public static event Action<ProgramCardData, int, Player> OnPlayerRegister;
    public static event Action<ExecutionSubPhase> OnNewSubPhase;
    public static event Action<IReadOnlyList<Player>> OnPlayersOrdered;
    public static event Action<int> OnNewRegister; 

    public static IEnumerator DoPhase() {
        OnPhaseStart?.Invoke();
        yield return UIManager.Instance.ChangeState(UIState.Execution);
        
        TaskScheduler.PushSequence(
            delay: StepDelay,
            routines: EnumerableUtils.For(RegisterCount, DoRegister).ToArray()
            );

        yield return TaskScheduler.WaitUntilClear();
        
        OnExecutionComplete?.Invoke();
        
        foreach (var player in PlayerSystem.Players) {
            player.DiscardProgram();
        }
        
        OnPhaseEnd?.Invoke();
    }

    // ReSharper disable Unity.PerformanceAnalysis
    static IEnumerator DoRegister(int register) {
        CurrentRegister = register;
        OnNewRegister?.Invoke(register);

        yield return CoroutineUtils.Wait(StepDelay);

        var players = PlayerSystem.GetOrderedPlayers().ToArray();
        OnPlayersOrdered?.Invoke(players);
        
        yield return CoroutineUtils.Wait(StepDelay);

        var registerRoutines = new IEnumerator[players.Length];
        for (var i = 0; i < players.Length; i++) {
            registerRoutines[i] = DoPlayerRegister(players[i]);
        }

        TaskScheduler.PushSequence(
            delay: SubPhaseDelay,
            DoSubPhase(ExecutionSubPhase.Registers, () => {
                TaskScheduler.PushSequence(routines: registerRoutines);
                return players.Any(p => !p.IsRebooted.Value);
            }),
            DoSubPhase(ExecutionSubPhase.Conveyor, Conveyor.ActivateElement),
            DoSubPhase(ExecutionSubPhase.PushPanel, PushPanel.ActivateElement),
            DoSubPhase(ExecutionSubPhase.Gear, Gear.ActivateElement),
            DoSubPhase(ExecutionSubPhase.BoardLaser, BoardLaser.ActivateElement),
            DoSubPhase(ExecutionSubPhase.PlayerLaser, () => {
                TaskScheduler.PushSequence(
                    routines: players
                        .Where(p => !p.IsRebooted.Value)
                        .Select(p => p.Model.FireLaser(p.Model.Rotator.Identity))
                        .ToArray()
                );
                return players.Any(p => !p.IsRebooted.Value);
            }),
            DoSubPhase(ExecutionSubPhase.EnergySpace, EnergySpace.ActivateElement),
            DoSubPhase(ExecutionSubPhase.Checkpoint, Checkpoint.ActivateElement)
        );

        IEnumerator DoSubPhase(ExecutionSubPhase subPhase, Func<bool> execute) {
            CurrentSubPhase = subPhase;
            OnNewSubPhase?.Invoke(subPhase);
            if (execute()) {
                yield return CoroutineUtils.Wait(SubPhaseDelay);
            }
        }

        IEnumerator DoPlayerRegister(Player player) {
            CurrentPlayer = player;
            var card = player.Program[register];
            if (card == null) yield break;
            
            OnPlayerRegister?.Invoke(card, register, player);
            yield return card.ExecuteRoutine(player, register);
        }
    }
}

public enum ExecutionSubPhase {
    Registers,
    Conveyor,
    PushPanel,
    Gear,
    BoardLaser,
    PlayerLaser,
    EnergySpace,
    Checkpoint,
}
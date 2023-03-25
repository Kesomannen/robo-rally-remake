using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ExecutionPhase : NetworkSingleton<ExecutionPhase> {
    public static int CurrentRegister { get; private set; }

    public const int RegisterCount = 5;

    public static event Action PhaseStart, PhaseEnd, ExecutionComplete, PlayerRegistersComplete;
    public static event Action<ExecutionSubPhase> NewSubPhase;
    public static event Action<IReadOnlyList<Player>> PlayersOrdered;
    public static event Action<int> NewRegister;

    public static event Action<ProgramExecution> PlayerRegister;

    public static IEnumerable<Action> GetPhaseEndInvocations() {
        var invocationList = PhaseEnd?.GetInvocationList();
        return invocationList?.Cast<Action>();
    }

    public IEnumerator DoPhase() {
        yield return UIManager.Instance.ChangeState(UIState.Execution);
        PhaseStart?.Invoke();
        
        TaskScheduler.PushSequence(routines: EnumerableUtils.For(RegisterCount, DoRegister).ToArray());
        yield return TaskScheduler.WaitUntilClear();
        ExecutionComplete?.Invoke();
        foreach (var player in PlayerSystem.Players) {
            player.DiscardProgram();
        }
        yield return TaskScheduler.WaitUntilClear();

        PhaseEnd?.Invoke();
    }

    static Player[] _previousPlayerOrder;

    static IEnumerator DoRegister(int register) {
         CurrentRegister = register; 
         NewRegister?.Invoke(register);

        yield return CoroutineUtils.Wait(TaskScheduler.DefaultTaskDelay);
        yield return UpgradeAwaiter.AwaitEvent(UpgradeAwaiter.BeforePlayerOrdering);

        var orderedPlayers = PlayerSystem.Instance.GetOrderedPlayers().ToArray();
        if (_previousPlayerOrder != null && !_previousPlayerOrder.SequenceEqual(orderedPlayers)) {
            PlayersOrdered?.Invoke(orderedPlayers);
            yield return CoroutineUtils.Wait(TaskScheduler.DefaultTaskDelay);
        }
        _previousPlayerOrder = orderedPlayers;
        
        var registerRoutines = new IEnumerator[orderedPlayers.Length];
        for (var i = 0; i < orderedPlayers.Length; i++) {
            registerRoutines[i] = DoPlayerRegister(orderedPlayers[i]);
        }

        TaskScheduler.PushSequence(
            delay: 0,
            DoSubPhase(ExecutionSubPhase.Registers, () => {
                TaskScheduler.PushSequence(routines: registerRoutines);
                return PlayerSystem.Players.Any(p => !p.IsRebooted.Value);
            }),
            CompletePlayerRegisters(),
            DoSubPhase(ExecutionSubPhase.Conveyor, Conveyor.ActivateElement),
            DoSubPhase(ExecutionSubPhase.PushPanel, PushPanel.ActivateElement),
            DoSubPhase(ExecutionSubPhase.Gear, Gear.ActivateElement),
            DoSubPhase(ExecutionSubPhase.BoardLaser, BoardLaser.ActivateElement),
            DoSubPhase(ExecutionSubPhase.PlayerLaser, PlayerModel.ShootLasers),
            DoSubPhase(ExecutionSubPhase.EnergySpace, EnergySpace.ActivateElement),
            DoSubPhase(ExecutionSubPhase.Checkpoint, Checkpoint.ActivateElement)
        );

        IEnumerator CompletePlayerRegisters() {
            PlayerRegistersComplete?.Invoke();
            yield break;
        }
        
        IEnumerator DoSubPhase(ExecutionSubPhase subPhase, Func<bool> execute) {
            if (execute()) {
                NewSubPhase?.Invoke(subPhase);
            }
            yield break;
        }

        IEnumerator DoPlayerRegister(Player player) {
            if (player.IsRebooted.Value) yield break;
            Debug.Log($"Starting player {player} register {register}");
            
            var execution = new ProgramExecution(() => player.Program[register], player, register);
            TaskScheduler.PushRoutine(UpgradeAwaiter.AwaitEvent(UpgradeAwaiter.AfterRegister, execution.Player));
            TaskScheduler.PushRoutine(execution.Execute());
            
            PlayerRegister?.Invoke(execution);
            yield return UpgradeAwaiter.AwaitEvent(UpgradeAwaiter.BeforeRegister, player);
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
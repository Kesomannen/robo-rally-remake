using System.Collections;
using System;

public class ExecutePhase : Phase<ExecutePhase> {
    public static int CurrentRegister { get; private set; }
    public static Player CurrentPlayer { get; private set; }

    public const int RegisterCount = 5;

    public event Action<int> OnRegisterStart, OnRegisterEnd;
    public override event Action OnPhaseStart, OnPhaseEnd;

    public override IEnumerator DoPhase() {
        OnPhaseStart?.Invoke();
        for (CurrentRegister = 0; CurrentRegister < RegisterCount; CurrentRegister++) {
            foreach (var player in PlayerManager.OrderPlayers()) {
                CurrentPlayer = player;
                player.Registers[CurrentRegister].Execute(player, CurrentRegister);
                yield return Scheduler.WaitUntilQueueEmpty();
            }
            OnRegisterEnd?.Invoke(CurrentRegister);
        }
        OnPhaseEnd?.Invoke();
    }
}
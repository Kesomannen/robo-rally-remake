using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade/Hack")]
public class HackUpgrade : UpgradeCardData {
    public override void OnAdd(Player player) {
        UpgradeAwaiter.AfterRegister.AddListener(player);
    }
    
    public override void OnRemove(Player player) {
        UpgradeAwaiter.AfterRegister.RemoveListener(player);
    }

    public override bool CanUse(Player player) {
        return UpgradeAwaiter.AfterRegister.ActiveFor(player);
    }
    
    public override void Use(Player player) {
        TaskScheduler.PushRoutine(Task());
        
        IEnumerator Task() {
            var register = ExecutionPhase.CurrentRegister;
            var card = player.Program[register];
            Log.Instance.RawMessage($"{Log.PlayerString(player)} executes {Log.ProgramString(card)} again");
            yield return new ProgramExecution(() => card, player, register).Execute();
        }
    }
}
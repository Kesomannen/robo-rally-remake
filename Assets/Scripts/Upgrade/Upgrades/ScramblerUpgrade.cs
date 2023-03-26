using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade/Scrambler")]
public class ScramblerUpgrade : UpgradeCardData {
    public override void OnAdd(Player target) {
        target.Owner.Model.OnShoot += OnShoot;
    }
    
    public override void OnRemove(Player target) {
        target.Owner.Model.OnShoot -= OnShoot;
    }

    void OnShoot(PlayerModel.CallbackContext context) {
        var register = ExecutionPhase.CurrentRegister;
        if (register >= ExecutionPhase.RegisterCount - 1) return;
        context.Target.Program.SetRegister(register + 1, null);
        
        Log.Message($"{Log.PlayerString(context.Attacker)} discarded register {register + 1} of {Log.PlayerString(context.Target)} with {Log.UpgradeString(this)}");
    }
}
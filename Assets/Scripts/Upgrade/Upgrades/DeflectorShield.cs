using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade/Deflector Shield")]
public class DeflectorShield : UpgradeCardData {
    public override void OnAdd(Player player) {
        UpgradeAwaiter.BeforeRegister.AddListener(player);
    }
    
    public override void OnRemove(Player player) {
        UpgradeAwaiter.BeforeRegister.RemoveListener(player);
    }

    public override bool CanUse(Player player) {
        return player.Energy.Value >= 1 && UpgradeAwaiter.BeforeRegister.ActiveFor(player);
    }
    
    public override void Use(Player player) {
        ExecutionPhase.OnNewRegister += NewRegister;
        player.Model.InvulnerableToLasers = true;
        player.Energy.Value--;

        void NewRegister(int register) {
            ExecutionPhase.OnNewRegister -= NewRegister;
            player.Model.InvulnerableToLasers = false;
        }
    }
}
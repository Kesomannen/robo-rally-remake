using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade/Reboot")]
public class RebootUpgrade : UpgradeCardData {
    [SerializeField] bool _takeDamage;
    
    public override bool CanUse(Player player) {
        return UpgradeAwaiter.AfterRegister.ActiveFor(player);
    }

    public override void OnAdd(Player player) {
        UpgradeAwaiter.AfterRegister.AddListener(player);
    }

    public override void Use(Player player) {
        player.RebootFromParentBoard(_takeDamage);
    }
}
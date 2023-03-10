using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade/Reboot")]
public class RebootUpgrade : UpgradeCardData {
    [SerializeField] bool _takeDamage;
    
    public override bool CanUse(Player player) {
        return UpgradeSystem.BeforeRegister.Active;
    }

    public override void OnAdd(Player player) {
        UpgradeSystem.BeforeRegister.AddListener();
    }

    public override void Use(Player player) {
        player.RebootFromParentBoard(_takeDamage);
    }
}
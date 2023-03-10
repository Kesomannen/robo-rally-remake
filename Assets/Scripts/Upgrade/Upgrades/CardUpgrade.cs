using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade/Card")]
public class CardUpgrade : UpgradeCardData {
    [SerializeField] CardAffector _cardAffector;

    public override bool CanUse(Player player) {
        return UpgradeUtils.IsProgramming(player);
    }

    public override void Use(Player player) {
        player.ApplyCardAffector(_cardAffector);
    }
}
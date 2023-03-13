using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade/Recompile")]
public class RecompileUpgrade : UpgradeCardData {
    public override bool CanUse(Player player) {
        return UpgradeUtils.IsProgramming(player);
    }

    public override void Use(Player player) {
        var cards = player.Hand.Cards.Count;
        player.DiscardHand();
        player.DrawCards(cards);
    }
}
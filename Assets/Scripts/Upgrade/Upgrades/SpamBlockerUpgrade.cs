using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade/Spam Blocker")]
public class SpamBlockerUpgrade : UpgradeCardData {
    public override bool CanUse(Player player) {
        return UpgradeUtils.IsProgramming(player);
    }

    public override void Use(Player player) {
        var cards = player.Hand.Cards.Where(c => c.Type == ProgramCardData.CardType.Damage).ToArray();
        if (cards.Length == 0) return;
        foreach (var card in cards) {
            player.DiscardCard(card);
        }
        player.DrawCards(cards.Length);
        
        Log.Instance.RawMessage($"{Log.PlayerString(player)} discarded {cards.Length} damage cards");
    }
}
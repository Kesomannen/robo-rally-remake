using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade/Card")]
public class CardUpgrade : UpgradeCardData {
    [SerializeField] CardAffector _cardAffector;
    
    public override bool CanUse(Player player) {
        return UpgradeUtils.IsProgramming(player);
    }

    public override void Use(Player player) {
        player.ApplyCardAffector(_cardAffector);
        var pileName = _cardAffector.Destination switch {
            Pile.Hand => "hand",
            Pile.DiscardPile => "discard pile",
            Pile.DrawPile => "draw pile",
            _ => "unknown pile"
        };
        Log.Instance.RawMessage($"{Log.PlayerString(player)} added a {Log.ProgramString(_cardAffector.Cards[0])} to their {pileName}");
    }
}
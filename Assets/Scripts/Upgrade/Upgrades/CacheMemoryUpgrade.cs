using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade/Cache Memory")]
public class CacheMemoryUpgrade : UpgradeCardData {
    [SerializeField] OverlayData<Choice<ProgramCardData>> _overlay;

    public override bool CanUse(Player player) {
        return UpgradeUtils.IsProgramming(player) && player.Hand.Cards.Count > 0;
    }

    public override void Use(Player player) {
        var cards = player.Hand.Cards;
        if (cards.Count == 1) {
            var card = cards[0];
            player.Hand.RemoveCard(card);
            player.DrawPile.AddCard(card, CardPlacement.Top);
        } else {
            TaskScheduler.PushRoutine(Task());
        }

        IEnumerator Task() {
            var result = new ProgramCardData[cards.Count];
            yield return ChoiceSystem.DoChoice(new ChoiceData<ProgramCardData> {
                Overlay = _overlay,
                Player = player,
                Options = cards,
                Message = "choosing cards to discard with Cache Memory",
                OutputArray = result,
                MinChoices = 1
            });
            foreach (var card in result) {
                player.Hand.RemoveCard(card);
                player.DrawPile.AddCard(card, CardPlacement.Top);
            }
        }
    }
}
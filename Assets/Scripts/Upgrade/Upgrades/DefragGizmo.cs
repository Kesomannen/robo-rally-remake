using System.Collections;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade/Defrag Gizmo")]
public class DefragGizmo : UpgradeCardData {
    [SerializeField] OverlayData<Choice<ProgramCardData>> _overlay;

    public override bool CanUse(Player player) {
        return UpgradeUtils.IsProgramming(player) &&
               player.Hand.Cards.Any(card => card.Type == ProgramCardData.CardType.Damage);
    }

    public override void Use(Player player) {
        var cards = player.Hand.Cards.Where(card => card.Type == ProgramCardData.CardType.Damage).ToArray();
        if (cards.Length == 1) {
            player.DiscardCard(cards[0]);
            player.DrawCards(1);
        } else {
            TaskScheduler.PushRoutine(Task());
        }

        IEnumerator Task() {
            var result = new ProgramCardData[1];
            yield return ChoiceSystem.DoChoice(new ChoiceData<ProgramCardData> {
                Overlay = _overlay,
                Player = player,
                Options = cards,
                Message = "choosing a card to replace with Defrag Gizmo",
                OutputArray = result,
                MinChoices = 1
            });
            player.DiscardCard(result[0]);
            player.DrawCards(1);
        }
    }
}
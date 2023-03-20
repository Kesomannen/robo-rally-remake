using System.Collections;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade/Cache Memory")]
public class CacheMemoryUpgrade : UpgradeCardData {
    [SerializeField] OverlayData<Choice<ProgramCardData>> _overlay;

    public override bool CanUse(Player player) {
        return UpgradeUtils.IsProgramming(player) && player.Hand.Cards.Count > 0;
    }

    public override void Use(Player player) {
        // Make sure the player can still finish their program
        var cards = player.Hand.Cards;
        var cardsInProgram = player.Program.Cards.Count(c => c != null);
        var maxDiscards = Mathf.Min(cards.Count, cardsInProgram + cards.Count - ExecutionPhase.RegisterCount);
        
        if (maxDiscards == 0) return;
        TaskScheduler.PushRoutine(Task());

        IEnumerator Task() {
            var result = new ProgramCardData[maxDiscards];
            yield return ChoiceSystem.DoChoice(new ChoiceData<ProgramCardData> {
                Overlay = _overlay,
                Player = player,
                Options = cards,
                Message = "choosing cards to cache with Cache Memory",
                OutputArray = result,
                MinChoices = 1
            });
            foreach (var card in result) {
                player.Hand.RemoveCard(card);
                player.DrawPile.AddCard(card, CardPlacement.Top);
            }
            Log.Instance.RawMessage($"{Log.PlayerString(player)} cached {string.Join(",", result.Select(Log.ProgramString))}");
        }
    }
}
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade/Memory Swap")]
public class MemorySwapUpgrade : UpgradeCardData {
    [SerializeField] int _cardCount;
    [SerializeField] OverlayData<Choice<ProgramCardData>> _overlay;

    public override bool CanUse(Player player) {
        return UpgradeUtils.IsProgramming(player);
    }

    public override void Use(Player player) {
        player.DrawCards(_cardCount);
        TaskScheduler.PushRoutine(Task());
        
        IEnumerator Task() {
            var result = new ProgramCardData[_cardCount];
            yield return ChoiceSystem.DoChoice(new ChoiceData<ProgramCardData> {
                Overlay = _overlay,
                Player = player,
                Options = player.Hand.Cards,
                Message = "choosing cards to cache with Memory Swap",
                OutputArray = result,
                MinChoices = 3
            });
            foreach (var card in result) {
                player.Hand.RemoveCard(card);
                player.DrawPile.AddCard(card, CardPlacement.Top);
            }
            
            Log.Instance.RawMessage($"{Log.PlayerString(player)} cached {Log.ProgramString(result[0])}, {Log.ProgramString(result[1])}, and {Log.ProgramString(result[2])}");
        }
    }
}
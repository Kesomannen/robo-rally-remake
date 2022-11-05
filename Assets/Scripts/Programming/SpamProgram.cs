using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "SpamProgram", menuName = "ScriptableObjects/Programs/Spam")]
public class SpamProgram : ProgramCardData {
    public override bool CanPlace(Player player, int positionInRegister) => true;

    public override IEnumerator ExecuteRoutine(Player player, int positionInRegister)  {
        while (true) {
            yield return Helpers.Wait(1);

            if (player.DrawPile.Cards.Count == 0) {
                player.ShuffleDeck();
            }

            var card = player.DrawPile[0];
            player.DrawPile.RemoveCard(0);

            if (card.CanPlace(player, positionInRegister)) {
                yield return card.ExecuteRoutine(player, positionInRegister);
                player.DiscardPile.AddCard(card, CardPlacement.Top);
                yield break;
            } else {
                player.DiscardPile.AddCard(card, CardPlacement.Top);
            }
        }
    }
}
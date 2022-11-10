using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "SpamProgram", menuName = "ScriptableObjects/Programs/Spam")]
public class SpamProgram : ProgramCardData {
    public override bool CanPlace(Player player, int positionInRegister) => true;

    public override IEnumerator ExecuteRoutine(Player player, int positionInRegister) {
        player.DiscardPile.AddCard(this, CardPlacement.Top);
        player.Program.SetCard(positionInRegister, null);

        ProgramCardData card;
        do {
            yield return Helpers.Wait(1);
            card = player.GetTopCard();
            player.DiscardPile.AddCard(card, CardPlacement.Top);
        } while (!card.CanPlace(player, positionInRegister));

        yield return card.ExecuteRoutine(player, positionInRegister);
    }
}
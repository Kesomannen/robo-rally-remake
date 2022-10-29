using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "SpamProgram", menuName = "ScriptableObjects/Programs/Spam")]
public class SpamProgram : ProgramCardData {
    public override bool CanPlace(Player player, int positionInRegister) => true;

    public override IEnumerator Execute(Player player, int positionInRegister)  {
        var card = player.DrawPile[0];
        player.DrawPile.RemoveCard(0);
        yield return card.Execute(player, positionInRegister);
        player.DiscardPile.AddCard(card, CardPlacement.Top);
    }
}
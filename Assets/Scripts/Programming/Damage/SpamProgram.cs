using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "SpamProgram", menuName = "ScriptableObjects/Programs/Spam")]
public class SpamProgram : ProgramCardData {
    public override bool CanPlace(Player player, int positionInRegister) => true;
    
    public override IEnumerator ExecuteRoutine(Player player, int positionInRegister) {
        ProgramCardData card;
        do {
            card = player.GetTopCard();
            player.DiscardPile.AddCard(card, CardPlacement.Top);
            yield return Helpers.Wait(1);
        } while (!card.CanPlace(player, positionInRegister));

        yield return card.ExecuteRoutine(player, positionInRegister);
        ExecutionPhase.OnExecutionComplete += RemoveSpamCard;

        void RemoveSpamCard() {
            ExecutionPhase.OnExecutionComplete -= RemoveSpamCard;
            player.Program.SetCard(positionInRegister, null);
        }
    }
}
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "SpamProgram", menuName = "ScriptableObjects/Programs/Spam")]
public class SpamProgram : ProgramCardData {
    [SerializeField] float _timeBetweenDraws;
    
    public override bool CanPlace(Player player, int positionInRegister) => true;
    
    public override IEnumerator ExecuteRoutine(Player player, int positionInRegister) {
        ProgramCardData card;
        do {
            card = player.GetTopCard();
            player.DiscardPile.AddCard(card, CardPlacement.Top);
            yield return CoroutineUtils.Wait(_timeBetweenDraws);
        } while (!card.CanPlace(player, positionInRegister));

        player.RegisterPlay(card);
        yield return card.ExecuteRoutine(player, positionInRegister);
        ExecutionPhase.OnExecutionComplete += RemoveCard;

        void RemoveCard() {
            Debug.Log("Removing card");
            ExecutionPhase.OnExecutionComplete -= RemoveCard;
            player.Program.SetCard(positionInRegister, null);
        }
    }
}
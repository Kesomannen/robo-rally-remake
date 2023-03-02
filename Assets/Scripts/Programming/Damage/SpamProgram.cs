using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "SpamProgram", menuName = "ScriptableObjects/Programs/Spam")]
public class SpamProgram : ProgramCardData {
    const int SearchDepth = 5;
    
    public override bool CanPlace(Player player, int register) => true;
    
    public override IEnumerator ExecuteRoutine(Player player, int register) {
        var i = 0;
        List<ProgramCardData> cards = new();

        do {
            cards.Clear();
            yield return NetworkSystem.Instance.QueryPlayerCards(player, Pile.DrawPile, i * SearchDepth, (i + 1) * SearchDepth, cards);
            i++;
        } while (!cards.Any(c => c.CanPlace(player, register)));
        var card = cards.First(c => c.CanPlace(player, register));
        player.DrawPile.RemoveCard(card);
        
        yield return CoroutineUtils.Wait(0.5f);
        player.RegisterPlay(card);
        yield return card.ExecuteRoutine(player, register);
        ExecutionPhase.OnExecutionComplete += RemoveCard;
        
        player.DiscardPile.AddCard(card, CardPlacement.Top);

        void RemoveCard() {
            Debug.Log("Removing card");
            ExecutionPhase.OnExecutionComplete -= RemoveCard;
            player.Program.SetRegister(register, null);
        }
    }
}
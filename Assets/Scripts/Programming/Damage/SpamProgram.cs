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
            yield return NetworkUtils.Instance.QueryPlayerCards(player, Pile.DrawPile, i * SearchDepth, (i + 1) * SearchDepth, cards);
            i++;
        } while (!cards.Any(c => c.CanPlace(player, register)));
        var card = cards.First(c => c.CanPlace(player, register));
        player.DrawPile.RemoveCard(card);
        
        yield return CoroutineUtils.Wait(0.5f);
        yield return new ProgramExecution(() => card, player, register).Execute();
        ExecutionPhase.ExecutionComplete += RemoveCard;
        
        player.DiscardPile.AddCard(card, CardPlacement.Top);

        void RemoveCard() {
            ExecutionPhase.ExecutionComplete -= RemoveCard;
            
            var program = player.Program;
            if (program[register] != this) return;
            program.SetRegister(register, null);
        }
    }
}
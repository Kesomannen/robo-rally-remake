using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrade/Scrambler")]
public class ScramblerUpgrade : UpgradeCardData {
    const int SearchDepth = 5;
    
    public override void OnAdd(Player target) {
        target.Owner.Model.OnShoot += OnShoot;
    }
    
    public override void OnRemove(Player target) {
        target.Owner.Model.OnShoot -= OnShoot;
    }

    static void OnShoot(PlayerModel.CallbackContext context) {
        var register = ExecutionPhase.CurrentRegister;
        if (register >= ExecutionPhase.RegisterCount - 1) return;
        
        TaskScheduler.PushRoutine(Task());

        IEnumerator Task() {
            var target = context.Target;
            var i = 0;
            List<ProgramCardData> cards = new();

            do {
                cards.Clear();
                yield return NetworkSystem.Instance.QueryPlayerCards(target, Pile.DrawPile, i * SearchDepth, (i + 1) * SearchDepth, cards);
                i++;
            } while (!cards.Any(c => c.CanPlace(target, register)));
            var card = cards.First(c => c.CanPlace(target, register));
            target.DrawPile.RemoveCard(card);
        
            yield return CoroutineUtils.Wait(0.5f);
            target.Program.SetRegister(register + 1, card);
        }
    }
}
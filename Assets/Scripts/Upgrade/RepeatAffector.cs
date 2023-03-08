using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Affectors/Repeat")]
public class RepeatAffector : ScriptablePermanentAffector<IPlayer> {
    [SerializeField] int _repeatCount;
    
    public override void Apply(IPlayer player) {
        var register = ExecutionPhase.CurrentRegister;
        var card = player.Owner.Program[register];
        if (card == null) return;
        
        TaskScheduler.PushRoutine(Task());
        
        IEnumerator Task() {
            for (var i = 0; i < _repeatCount; i++) {
                player.Owner.RegisterPlay(card);
                yield return card.ExecuteRoutine(player.Owner, register);
            }
        }
    }
}
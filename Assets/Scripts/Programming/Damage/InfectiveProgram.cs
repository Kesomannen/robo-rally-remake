using System.Collections;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "InfectiveProgram", menuName = "ScriptableObjects/Programs/Infective")]
public class InfectiveProgram : SpamProgram {
    [SerializeField] int _infectionRange;
    [SerializeField] ScriptablePermanentAffector<IPlayer> _affector;
    
    public override IEnumerator ExecuteRoutine(Player player, int register) {
        foreach (var plr in PlayerSystem.Players.Where(plr => plr != player)) {
            if (plr.Model.GridPos.GridDistance(player.Model.GridPos) <= _infectionRange){
                _affector.Apply(plr);
            }
        }
        yield return base.ExecuteRoutine(player, register);
    }
}
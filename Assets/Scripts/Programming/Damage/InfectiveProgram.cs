using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "InfectiveProgram", menuName = "ScriptableObjects/Programs/Infective")]
public class InfectiveProgram : SpamProgram {
    [SerializeField] int _infectionRange;
    [SerializeField] ScriptablePermanentAffector<IPlayer> _affector;
    
    public override IEnumerator ExecuteRoutine(Player player, int register) {
        foreach (var plr in PlayerSystem.Players){
            if (plr == player) continue;
            if (Vector2Int.Distance(plr.Model.GridPos, player.Model.GridPos) <= _infectionRange){
                _affector.Apply(plr);
            }
        }
        yield return base.ExecuteRoutine(player, register);
    }
}
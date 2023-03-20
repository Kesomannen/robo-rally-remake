using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageAffectorProgram", menuName = "ScriptableObjects/Programs/DamageAffector")]
public class DamageAffectorProgram : SpamProgram {
    [SerializeField] ScriptablePermanentAffector<Player> _affector;
    [SerializeField] bool _actAsSpam;

    public override bool CanPlace(Player player, int register) => true;

    public override IEnumerator ExecuteRoutine(Player player, int register) {
        _affector.Apply(player);
        if (_actAsSpam) yield return base.ExecuteRoutine(player, register);
        else ExecutionPhase.ExecutionComplete += RemoveCard;

        void RemoveCard() {
            ExecutionPhase.ExecutionComplete -= RemoveCard;
            player.Program.SetRegister(register, null);
        }
    }
}
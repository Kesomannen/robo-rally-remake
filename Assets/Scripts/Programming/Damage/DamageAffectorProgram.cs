using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageAffectorProgram", menuName = "ScriptableObjects/Programs/DamageAffector")]
public class DamageAffectorProgram : SpamProgram {
    [SerializeField] ScriptablePermanentAffector<IPlayer> _affector;
    [SerializeField] bool _actAsSpam;

    public override bool CanPlace(Player player, int positionInRegister) => true;

    public override IEnumerator ExecuteRoutine(Player player, int positionInRegister) {
        _affector.Apply(player);
        if (_actAsSpam) yield return base.ExecuteRoutine(player, positionInRegister);
        else ExecutionPhase.OnExecutionComplete += RemoveCard;

        void RemoveCard() {
            ExecutionPhase.OnExecutionComplete -= RemoveCard;
            player.Program.SetCard(positionInRegister, null);
        }
    }
}
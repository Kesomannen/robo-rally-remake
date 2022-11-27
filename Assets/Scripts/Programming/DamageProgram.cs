using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageProgram", menuName = "ScriptableObjects/Programs/Damage")]
public class DamageProgram : SpamProgram {
    [SerializeField] CardAffector _cardAffector;

    public override bool CanPlace(Player player, int positionInRegister) => true;

    public override IEnumerator ExecuteRoutine(Player player, int positionInRegister) {
        _cardAffector.Apply(player);
        yield return base.ExecuteRoutine(player, positionInRegister);
    }
}
using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageProgram", menuName = "ScriptableObjects/Programs/Damage")]
public class DamageProgram : SpamProgram {
    [SerializeField] Damage _damage;

    public override bool CanPlace(Player player, int positionInRegister) => true;

    public override IEnumerator ExecuteRoutine(Player player, int positionInRegister) {
        _damage.Apply(player);
        yield return base.ExecuteRoutine(player, positionInRegister);
    }
}
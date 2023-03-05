using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "HullDamageProgram", menuName = "ScriptableObjects/Programs/Hull Damage")]
public class HullDamageProgram : SpamProgram { 
    [SerializeField] int _relativeDiscardIndex;
    [SerializeField] bool _discardPermanently;

    public override bool CanPlace(Player player, int register) {
        return register + _relativeDiscardIndex is >= 0 and < ExecutionPhase.RegisterCount;
    }

    public override IEnumerator ExecuteRoutine(Player player, int register)  {
        var registerToDiscard = register + _relativeDiscardIndex;
        if (_discardPermanently) {
            player.Program.SetRegister(registerToDiscard, null);
        } else {
            player.DiscardPile.AddCard(player.Program[registerToDiscard], CardPlacement.Top);
            player.Program.SetRegister(registerToDiscard, null);
        }
        
        yield return base.ExecuteRoutine(player, register);
    }
}
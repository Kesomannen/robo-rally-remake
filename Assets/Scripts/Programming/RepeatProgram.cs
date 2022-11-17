using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "RepeatProgram", menuName = "ScriptableObjects/Programs/Program")]
public class RepeatProgram : ProgramCardData {
    [SerializeField] int _repeatCount;
    [SerializeField] int _relativeRepeatIndex;

    public override bool CanPlace(Player player, int positionInRegister) {
        var posToRepeat = positionInRegister + _relativeRepeatIndex;
        return posToRepeat >= 0 && posToRepeat < ExecutionPhase.RegisterCount;
    }

    public override IEnumerator ExecuteRoutine(Player player, int positionInRegister)  {
        var posToRepeat = positionInRegister + _relativeRepeatIndex;
        for (var i = 0; i < _repeatCount; i++) {
            var card = player.Program[posToRepeat];
            yield return card.ExecuteRoutine(player, posToRepeat);
        }
    }
}
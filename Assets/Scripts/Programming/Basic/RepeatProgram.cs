using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "RepeatProgram", menuName = "ScriptableObjects/Programs/Program")]
public class RepeatProgram : ProgramCardData {
    [SerializeField] int _repeatCount;
    [SerializeField] int _relativeRepeatIndex;

    public override bool CanPlace(Player player, int positionInRegister) {
        return positionInRegister + _relativeRepeatIndex is >= 0 and < ExecutionPhase.RegisterCount;
    }

    public override IEnumerator ExecuteRoutine(Player player, int positionInRegister)  {
        var posToRepeat = positionInRegister + _relativeRepeatIndex;
        var card = player.Program[posToRepeat];
        for (var i = 0; i < _repeatCount; i++) {
            yield return card.ExecuteRoutine(player, posToRepeat);
        }
    }
}
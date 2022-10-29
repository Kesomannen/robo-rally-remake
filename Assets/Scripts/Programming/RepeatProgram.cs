using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "RepeatProgram", menuName = "ScriptableObjects/Programs/Program")]
public class RepeatProgram : ProgramCardData {
    [SerializeField] int repeatCount;
    [SerializeField] int relativeRepeatIndex;

    public override bool CanPlace(Player player, int positionInRegister) {
        var posToRepeat = positionInRegister + relativeRepeatIndex;
        return posToRepeat >= 0 && posToRepeat < ExecutionPhase.RegisterCount;
    }

    public override IEnumerator Execute(Player player, int positionInRegister)  {
        var posToRepeat = positionInRegister + relativeRepeatIndex;
        for (int i = 0; i < repeatCount; i++) {
            var card = player.Registers[posToRepeat].Card.Data;
            yield return card.Execute(player, posToRepeat);
        }
    }
}
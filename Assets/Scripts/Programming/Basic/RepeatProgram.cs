using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "RepeatProgram", menuName = "ScriptableObjects/Programs/Program")]
public class RepeatProgram : ProgramCardData {
    [SerializeField] int _repeatCount;
    [SerializeField] int _relativeRepeatIndex;
    [SerializeField] float _timeBetweenRepeats;

    public override bool CanPlace(Player player, int register) {
        return register + _relativeRepeatIndex is >= 0 and < ExecutionPhase.RegisterCount;
    }

    public override IEnumerator ExecuteRoutine(Player player, int register)  {
        var registerToRepeat = register + _relativeRepeatIndex;
        var card = player.Program[registerToRepeat];
        for (var i = 0; i < _repeatCount; i++) {
            yield return CoroutineUtils.Wait(_timeBetweenRepeats);
            player.RegisterPlay(card);
            yield return card.ExecuteRoutine(player, registerToRepeat);
        }
    }
}
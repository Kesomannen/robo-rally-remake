using System.Collections;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ChoiceProgram", menuName = "ScriptableObjects/Programs/Choice")]
public class ChoiceProgram : ProgramCardData {
    [SerializeField] ProgramCardData[] _options;
    [SerializeField] OverlayData<Choice<ProgramCardData>> _choiceOverlay;

    public override bool CanPlace(Player player, int positionInRegister) => _options.Any(c => c.CanPlace(player, positionInRegister));

    public override IEnumerator ExecuteRoutine(Player player, int positionInRegister) {
        var result = new Choice<ProgramCardData>.ChoiceResult();
        yield return Choice<ProgramCardData>.Create(
            player,
            _choiceOverlay,
            _options,
            _options.Select(o => o.CanPlace(player, positionInRegister)).ToArray(),
            result
            );
        yield return result.Value.ExecuteRoutine(player, positionInRegister);
    }
}
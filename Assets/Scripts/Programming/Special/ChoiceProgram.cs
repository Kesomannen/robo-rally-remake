using System.Collections;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ChoiceProgram", menuName = "ScriptableObjects/Programs/Choice")]
public class ChoiceProgram : ProgramCardData {
    [SerializeField] ProgramCardData[] _options;
    [SerializeField] OverlayData<Choice<ProgramCardData>> _choiceOverlay;

    public override bool CanPlace(Player player, int register) => _options.Any(c => c.CanPlace(player, register));

    public override IEnumerator ExecuteRoutine(Player player, int register) {
        var result = new ProgramCardData[1];
        yield return ChoiceSystem.DoChoice(new ChoiceData<ProgramCardData> {
            Player = player,
            Overlay = _choiceOverlay,
            AvailablePredicate = c => c.CanPlace(player, register),
            OutputArray = result,
            Options = _options,
            Message = "choosing a program card to execute",
            MinChoices = 1
        });
        player.RegisterPlay(result[0]);
        yield return result[0].ExecuteRoutine(player, register);
    }
}
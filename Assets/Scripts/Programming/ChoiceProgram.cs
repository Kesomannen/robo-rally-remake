using System.Collections;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ChoiceProgram", menuName = "ScriptableObjects/Programs/Choice")]
public class ChoiceProgram : ProgramCardData {
    [SerializeField] ProgramCardData[] options;
    [SerializeField] OverlayData<ProgramCardChoice> choiceOverlay;

    bool _hasMadeChoice;
    IEnumerator _choiceRoutine;

    public override bool CanPlace(Player player, int positionInRegister) => options.Any(c => c.CanPlace(player, positionInRegister));

    public override IEnumerator ExecuteRoutine(Player player, int positionInRegister) {
        _hasMadeChoice = false;

        var availableArray = options.Select(c => c.CanPlace(player, positionInRegister)).ToArray();
        var obj = OverlaySystem.Instance.ShowOverlay(choiceOverlay);
        obj.Init(options, availableArray, r => {
            _choiceRoutine = r.Choice.ExecuteRoutine(player, positionInRegister);
            _hasMadeChoice = true;
        });

        yield return new WaitUntil(() => _hasMadeChoice);
        yield return _choiceRoutine;
    }
}